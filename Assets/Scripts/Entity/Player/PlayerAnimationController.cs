using System.Collections.Generic;
using NSMB.Utils;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerAnimationController : MonoBehaviourPun
{
    [SerializeField] private Avatar smallAvatar, largeAvatar;
    [SerializeField] private ParticleSystem dust, sparkles, drillParticle, giantParticle, fireParticle;

    [SerializeField]
    private GameObject models, smallModel, largeModel, largeShellExclude, blueShell, propellerHelmet, propeller;

    [SerializeField] private Material glowMaterial;
    [SerializeField] private Color primaryColor = Color.clear, secondaryColor = Color.clear;
    [SerializeField] private float blinkDuration = 0.1f, deathUpTime = 0.6f, deathForce = 7f;
    public float pipeDuration = 2f;
    [FormerlySerializedAs("isHopper")] public bool excludeMaterialForSmall;
    [SerializeField] private AudioClip normalDrill, propellerDrill;
    public bool deathUp, wasTurnaround, enableGlow;

    private readonly Vector3 inShellPos = new(0f, -.0008f, -.0039f);

    private readonly Vector3 inShellRot = new(0f, 90f, 89.454f);

    private readonly Vector3 inShellSca = new(.85f, .85f, .85f);
    private readonly List<Renderer> renderers = new();

    [SerializeField] [ColorUsage(true, false)]
    private Color? _glowColor;

    private Animator animator;
    private float blinkTimer, pipeTimer, doorTimer, deathTimer, propellerVelocity;
    private Rigidbody2D body;

    private PlayerController controller;
    private readonly float doorDuration = 2.6f;

    private AudioSource drillParticleAudio;

    private Enums.PlayerEyeState eyeState;
    private SkinnedMeshRenderer largeMesh;
    private BoxCollider2D mainHitbox;
    private MaterialPropertyBlock materialBlock;

    private Vector3 outShellPos,
        outShellRot,
        outShellSca;

    private Material[] rememberedMaterialsLarge, rememberedMaterialsSmall;
    private bool useSpecialSmall;


    public Color GlowColor
    {
        get
        {
            if (_glowColor == null)
                _glowColor = Utils.GetPlayerColor(photonView.Owner);

            return _glowColor ?? Color.white;
        }
        set => _glowColor = value;
    }

    public void Start()
    {
        controller = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        mainHitbox = GetComponent<BoxCollider2D>();
        drillParticleAudio = drillParticle.GetComponent<AudioSource>();
        largeMesh = largeModel.GetComponentInChildren<SkinnedMeshRenderer>();

        DisableAllModels();

        if (photonView)
        {
            enableGlow = false;
            if (!photonView.IsMine) GameManager.Instance.CreateNametag(controller);

            var colorSet =
                GlobalController.Instance.skins[
                    (int)photonView.Owner.CustomProperties[Enums.NetPlayerProperties.PlayerColor]];
            if (colorSet != null)
            {
                var colors = colorSet.GetPlayerColors(controller.character);
                primaryColor = colors.overallsColor.linear;
                secondaryColor = colors.hatColor.linear;
            }
        }

        if (smallModel == largeModel) useSpecialSmall = true;
        if (excludeMaterialForSmall)
        {
            rememberedMaterialsLarge = largeMesh.materials;
            rememberedMaterialsSmall = new[] { largeMesh.materials[0], largeMesh.materials[1] };
        }

        outShellPos = blueShell.transform.localPosition;
        outShellRot = blueShell.transform.localRotation.eulerAngles;
        outShellSca = blueShell.transform.localScale;
    }

    public void Update()
    {
        HandleAnimations();

        if (renderers.Count == 0)
        {
            renderers.AddRange(GetComponentsInChildren<MeshRenderer>(true));
            renderers.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>(true));
        }
    }

    public void HandleAnimations()
    {
        var gameover = GameManager.Instance.gameover;

        if (gameover)
            models.SetActive(true);

        var targetEuler = models.transform.eulerAngles;
        bool instant = false, changeFacing = false;
        if (!gameover && !controller.Frozen)
        {
            if (controller.knockback)
            {
                targetEuler = new Vector3(0, controller.facingRight ? 110 : 250, 0);
                instant = true;
            }
            else if (animator.GetBool("pipe"))
            {
                targetEuler = new Vector3(0, 180, 0);
                instant = true;
            }
            else if (controller.doorEntering && doorTimer < doorDuration / 1.33f)
            {
                targetEuler = new Vector3(0, controller.doorDirection ? 0 : 180, 0);
                instant = false;
            }
            else if (animator.GetBool("inShell") && (!controller.onSpinner || Mathf.Abs(body.velocity.x) > 0.3f))
            {
                targetEuler += Mathf.Abs(body.velocity.x) / controller.RunningMaxSpeed * Time.deltaTime *
                               new Vector3(0, 1800 * (controller.facingRight ? -1 : 1));
                instant = true;
            }
            else if (wasTurnaround || controller.skidding || controller.turnaround ||
                     animator.GetCurrentAnimatorStateInfo(0).IsName("turnaround"))
            {
                if (controller.facingRight ^
                    (animator.GetCurrentAnimatorStateInfo(0).IsName("turnaround") || controller.skidding))
                    targetEuler = new Vector3(0, 250, 0);
                else
                    targetEuler = new Vector3(0, 110, 0);
                instant = true;
            }
            else if (animator.GetBool("goalAnimReachedBottom"))
            {
                targetEuler = new Vector3(0, 180, 0);
            }
            else
            {
                if (controller.onSpinner && controller.onGround && Mathf.Abs(body.velocity.x) < 0.3f &&
                    !controller.holding)
                {
                    targetEuler += new Vector3(0, -1800, 0) * Time.deltaTime;
                    instant = true;
                    changeFacing = true;
                }
                else if (controller.flying || controller.propeller)
                {
                    targetEuler += new Vector3(0,
                        -1200 - controller.propellerTimer * 2000 - (controller.drill ? 800 : 0) +
                        (controller.propeller && controller.propellerSpinTimer <= 0 && body.velocity.y < 0 ? 800 : 0),
                        0) * Time.deltaTime;
                    instant = true;
                }
                else
                {
                    targetEuler = new Vector3(0, controller.facingRight ? 110 : 250, 0);
                }
            }

            propellerVelocity =
                Mathf.Clamp(
                    propellerVelocity + 1800 *
                    (controller.flying || controller.propeller || controller.usedPropellerThisJump ? -1 : 1) *
                    Time.deltaTime, -2500, -300);
            propeller.transform.Rotate(Vector3.forward, propellerVelocity * Time.deltaTime);

            wasTurnaround = animator.GetCurrentAnimatorStateInfo(0).IsName("turnaround");
        }

        if (!controller.Frozen && (!GameManager.Instance.gameover || (GameManager.Instance.gameover && (GameManager.Instance.winningPlayer == null || GameManager.Instance.winningPlayer.ActorNumber != photonView.Owner.ActorNumber))))
        {
            if (controller.dead)
            {
                if (animator.GetBool("firedeath") && deathTimer > deathUpTime)
                    targetEuler = new Vector3(-15, controller.facingRight ? 110 : 250, 0);
                else
                    targetEuler = new Vector3(0, 180, 0);
                instant = true;
            }

            if (instant || wasTurnaround)
            {
                models.transform.rotation = Quaternion.Euler(targetEuler);
            }
            else
            {
                var maxRotation = 2000f * Time.deltaTime;
                float x = models.transform.eulerAngles.x,
                    y = models.transform.eulerAngles.y,
                    z = models.transform.eulerAngles.z;
                x += Mathf.Clamp(targetEuler.x - x, -maxRotation, maxRotation);
                y += Mathf.Clamp(targetEuler.y - y, -maxRotation, maxRotation);
                z += Mathf.Clamp(targetEuler.z - z, -maxRotation, maxRotation);
                models.transform.rotation = Quaternion.Euler(x, y, z);
            }

            if (changeFacing)
                controller.facingRight = models.transform.eulerAngles.y < 180;
        }

        //Particles
        SetParticleEmission(dust,
            !gameover && (controller.wallSlideLeft || controller.wallSlideRight ||
                          (controller.onGround && (controller.skidding ||
                                                   (controller.crouching && Mathf.Abs(body.velocity.x) > 1))) ||
                          (controller.sliding && Mathf.Abs(body.velocity.x) > 0.2 && controller.onGround)) &&
            !controller.pipeEntering);
        SetParticleEmission(drillParticle, !gameover && controller.drill);
        if (controller.drill)
            drillParticleAudio.clip =
                controller.state == Enums.PowerupState.PropellerMushroom ? propellerDrill : normalDrill;
        SetParticleEmission(sparkles, !gameover && controller.invincible > 0);
        SetParticleEmission(giantParticle,
            !gameover && controller.state == Enums.PowerupState.MegaMushroom && controller.giantStartTimer <= 0);
        SetParticleEmission(fireParticle,
            !gameover && animator.GetBool("firedeath") && controller.dead && deathTimer > deathUpTime);

        //Blinking
        if (controller.dead)
        {
            eyeState = Enums.PlayerEyeState.Death;
        }
        else
        {
            if ((blinkTimer -= Time.fixedDeltaTime) < 0)
                blinkTimer = 3f + Random.value * 6f;
            if (blinkTimer < blinkDuration)
                eyeState = Enums.PlayerEyeState.HalfBlink;
            else if (blinkTimer < blinkDuration * 2f)
                eyeState = Enums.PlayerEyeState.FullBlink;
            else if (blinkTimer < blinkDuration * 3f)
                eyeState = Enums.PlayerEyeState.HalfBlink;
            else
                eyeState = Enums.PlayerEyeState.Normal;
        }

        if (controller.cameraController.IsControllingCamera)
            HorizontalCamera.OFFSET_TARGET = controller.flying || controller.propeller ? 0.5f : 0f;

        if (controller.crouching || controller.sliding || controller.skidding)
            dust.transform.localPosition = Vector2.zero;
        else if (controller.wallSlideLeft || controller.wallSlideRight)
            dust.transform.localPosition =
                new Vector2(mainHitbox.size.x * (3f / 4f) * (controller.wallSlideLeft ? -1 : 1),
                    mainHitbox.size.y * (3f / 4f));
    }

    private void SetParticleEmission(ParticleSystem particle, bool value)
    {
        if (value)
        {
            if (particle.isStopped)
                particle.Play();
        }
        else
        {
            if (particle.isPlaying)
                particle.Stop();
        }
    }

    public void UpdateAnimatorStates()
    {
        var right = controller.joystick.x > 0.35f;
        var left = controller.joystick.x < -0.35f;

        animator.SetBool("onLeft", controller.wallSlideLeft);
        animator.SetBool("onRight", controller.wallSlideRight);
        animator.SetBool("onGround", controller.onGround);
        animator.SetBool("invincible", controller.invincible > 0);
        animator.SetBool("skidding", controller.skidding);
        animator.SetBool("propeller", controller.propeller);
        animator.SetBool("propellerSpin", controller.propellerSpinTimer > 0);
        animator.SetBool("crouching", controller.crouching);
        animator.SetBool("groundpound", controller.groundpound);
        animator.SetBool("sliding", controller.sliding);
        animator.SetBool("knockback", controller.knockback);
        animator.SetBool("facingRight", left ^ right ? right : controller.facingRight);
        animator.SetBool("flying", controller.flying);
        animator.SetBool("drill", controller.drill);

        if (photonView.IsMine)
        {
            //Animation
            animator.SetBool("turnaround", controller.turnaround);
            var animatedVelocity = Mathf.Abs(body.velocity.x) +
                                   Mathf.Abs(body.velocity.y * Mathf.Sin(controller.floorAngle * Mathf.Deg2Rad)) *
                                   (Mathf.Sign(controller.floorAngle) == Mathf.Sign(body.velocity.x) ? 0 : 1);
            if (controller.stuckInBlock)
                animatedVelocity = 0;
            else if (controller.propeller)
                animatedVelocity = 2.5f;
            else if (controller.state == Enums.PowerupState.MegaMushroom && Mathf.Abs(controller.joystick.x) > .2f)
                animatedVelocity = 4.5f;
            else if (left ^ right && !controller.hitRight && !controller.hitLeft)
                animatedVelocity = Mathf.Max(3.5f, animatedVelocity);
            else if (controller.onIce) animatedVelocity = 0;
            animator.SetFloat("velocityX", animatedVelocity);
            animator.SetFloat("velocityY", body.velocity.y);
            animator.SetBool("doublejump", controller.doublejump);
            animator.SetBool("triplejump", controller.triplejump);
            animator.SetBool("holding", controller.holding != null);
            animator.SetBool("head carry", controller.holding != null && controller.holding is FrozenCube);
            animator.SetBool("pipe", controller.pipeEntering != null);
            animator.SetBool("blueshell", controller.state == Enums.PowerupState.BlueShell);
            animator.SetBool("mini", controller.state == Enums.PowerupState.MiniMushroom);
            animator.SetBool("mega", controller.state == Enums.PowerupState.MegaMushroom);
            animator.SetBool("inShell",
                controller.inShell || (controller.state == Enums.PowerupState.BlueShell &&
                                       (controller.crouching || controller.groundpound) &&
                                       controller.groundpoundCounter <= 0.15f));
        }
        else
        {
            //controller.wallSlideLeft = animator.GetBool("onLeft");
            //controller.wallSlideRight = animator.GetBool("onRight");
            //controller.onGround = animator.GetBool("onGround");
            //controller.skidding = animator.GetBool("skidding");
            //controller.groundpound = animator.GetBool("groundpound");
            controller.turnaround = animator.GetBool("turnaround");
            //controller.crouching = animator.GetBool("crouching");
            controller.invincible = animator.GetBool("invincible") ? 1f : 0f;
            //controller.flying = animator.GetBool("flying");
            //controller.drill = animator.GetBool("drill");
            //controller.sliding = animator.GetBool("sliding");
            //controller.facingRight = animator.GetBool("facingRight");
            //controller.propellerSpinTimer = animator.GetBool("propellerSpin") ? 1f : 0f;
        }

        if (controller.giantEndTimer > 0)
            transform.localScale = Vector3.one + Vector3.one *
                (Mathf.Min(1, controller.giantEndTimer / (controller.giantStartTime / 2f)) * 2.6f);
        else
            transform.localScale = controller.state switch
            {
                Enums.PowerupState.MiniMushroom => Vector3.one / 2,
                Enums.PowerupState.MegaMushroom => Vector3.one + Vector3.one *
                    (Mathf.Min(1, 1 - controller.giantStartTimer / controller.giantStartTime) * 2.6f),
                _ => Vector3.one
            };

        //Shader effects
        if (materialBlock == null)
            materialBlock = new MaterialPropertyBlock();

        materialBlock.SetFloat("RainbowEnabled", controller.invincible > 0 ? 1.1f : 0f);
        var ps = controller.state switch
        {
            Enums.PowerupState.FireFlower => 1,
            Enums.PowerupState.PropellerMushroom => 2,
            Enums.PowerupState.IceFlower => 3,
            _ => 0
        };
        materialBlock.SetFloat("PowerupState", ps);
        materialBlock.SetFloat("EyeState", (int)eyeState);
        materialBlock.SetFloat("ModelScale", transform.lossyScale.x);
        if (enableGlow)
            materialBlock.SetColor("GlowColor", GlowColor);

        //Customizeable player color
        materialBlock.SetVector("OverallsColor", primaryColor);
        materialBlock.SetVector("ShirtColor", secondaryColor);

        var giantMultiply = Vector3.one;
        if (controller.giantTimer > 0 && controller.giantTimer < 4)
        {
            var v = (Mathf.Sin(controller.giantTimer * 20f) + 1f) / 2f * 0.9f + 0.1f;
            giantMultiply = new Vector3(v, 1, v);
        }

        materialBlock.SetVector("MultiplyColor", giantMultiply);

        foreach (var r in renderers)
            r.SetPropertyBlock(materialBlock);

        //hit flash
        models.SetActive(GameManager.Instance.gameover || controller.dead || !(controller.hitInvincibilityCounter > 0 &&
                                                                               controller.hitInvincibilityCounter *
                                                                               (controller.hitInvincibilityCounter <=
                                                                                   0.75f
                                                                                       ? 5
                                                                                       : 2) % (blinkDuration * 2f) <
                                                                               blinkDuration));

        //Model changing
        var large = controller.state >= Enums.PowerupState.Mushroom;

        if (!useSpecialSmall)
        {
            largeModel.SetActive(large);
            smallModel.SetActive(!large);
        }
        else
        {
            largeModel.SetActive(true);
        }

        blueShell.SetActive(controller.state == Enums.PowerupState.BlueShell);

        var inShellAnim = animator.GetCurrentAnimatorStateInfo(0).IsName("in-shell");
        blueShell.transform.localScale = inShellAnim ? inShellSca : outShellSca;
        blueShell.transform.localRotation = Quaternion.Euler(inShellAnim ? inShellRot : outShellRot);
        blueShell.transform.localPosition = inShellAnim ? inShellPos : outShellPos;
        largeShellExclude.SetActive(!inShellAnim);
        propellerHelmet.SetActive(controller.state == Enums.PowerupState.PropellerMushroom);
        animator.avatar = large ? largeAvatar : smallAvatar;
        animator.runtimeAnimatorController =
            large ? controller.character.largeOverrides : controller.character.smallOverrides;

        HandleDeathAnimation();
        HandlePipeAnimation();
        HandleDoorAnimation();

        transform.position = new Vector3(transform.position.x, transform.position.y,
            animator.GetBool("pipe") || (controller.doorEntering && doorTimer < doorDuration / 1.33f) ? 1 : -1);
        if (excludeMaterialForSmall) largeMesh.materials = large ? rememberedMaterialsLarge : rememberedMaterialsSmall;
        else if (useSpecialSmall)
            largeModel.transform.GetChild(0).localScale = large ? new Vector3(1, 1, 1) : new Vector3(0.8f, 0.7f, 0.7f);
    }

    public void HandleDeathAnimation()
    {
        if (!controller.dead)
        {
            deathTimer = 0;
            return;
        }

        if (GameManager.Instance.Togglerizer.currentEffects.Contains("FastDeath"))
            deathTimer = 3f;
        else
            deathTimer += Time.fixedDeltaTime;

        if (deathTimer < deathUpTime)
        {
            deathUp = false;
            body.gravityScale = 0;
            body.velocity = Vector2.zero;
            animator.Play("deadstart");
        }
        else
        {
            if (!deathUp && body.position.y > GameManager.Instance.GetLevelMinY())
            {
                body.velocity = new Vector2(0, deathForce);
                deathUp = true;
                if (animator.GetBool("firedeath"))
                {
                    controller.PlaySound(Enums.Sounds.Player_Voice_LavaDeath);
                    controller.PlaySound(Enums.Sounds.Player_Sound_LavaHiss);
                }

                animator.SetTrigger("deathup");
            }

            body.gravityScale = 1.2f;
            body.velocity = new Vector2(0, Mathf.Max(-deathForce, body.velocity.y));
        }

        if (controller.photonView.IsMine && deathTimer + Time.fixedDeltaTime > 2.5f - 0.43f &&
            deathTimer < 2.5f - 0.43f)
            if (!GameManager.Instance.gameover) controller.fadeOut.FadeOutAndIn();

        if (photonView.IsMine && deathTimer >= 3f && !GameManager.Instance.gameover)
            photonView.RPC("PreRespawn", RpcTarget.All);

        if (body.position.y < GameManager.Instance.GetLevelMinY() - transform.lossyScale.y)
        {
            models.SetActive(false);
            body.velocity = Vector2.zero;
            body.gravityScale = 0;
        }
    }

    private void HandlePipeAnimation()
    {
        if (!photonView.IsMine)
            return;
        if (!controller.pipeEntering)
        {
            pipeTimer = 0;
            return;
        }

        controller.UpdateHitbox();

        var pe = controller.pipeEntering;

        body.isKinematic = true;
        body.velocity = controller.pipeDirection;

        if (pipeTimer < pipeDuration / 2f && pipeTimer + Time.fixedDeltaTime >= pipeDuration / 2f)
        {
            //tp to other pipe
            if (pe.otherPipe.bottom == pe.bottom)
                controller.pipeDirection *= -1;

            var offset = controller.pipeDirection * (pipeDuration / 2f);
            if (pe.otherPipe.bottom)
            {
                var size = controller.MainHitbox.size.y * transform.localScale.y;
                offset.y += size;
            }

            transform.position = body.position =
                new Vector3(pe.otherPipe.transform.position.x, pe.otherPipe.transform.position.y, 1) -
                (Vector3)offset;
            photonView.RPC("PlaySound", RpcTarget.All, Enums.Sounds.Player_Sound_Powerdown);
            // controller.cameraController.Recenter();
        }

        if (pipeTimer >= pipeDuration)
        {
            controller.pipeEntering = null;
            body.isKinematic = false;
            controller.onGround = false;
            controller.properJump = false;
            controller.koyoteTime = 1;
            controller.crouching = false;
            controller.alreadyGroundpounded = true;
            controller.pipeTimer = 0.25f;
            body.velocity = Vector2.zero;
        }

        pipeTimer += Time.fixedDeltaTime;
    }

    private void HandleDoorAnimation()
    {
        if (!photonView.IsMine)
            return;
        if (!controller.doorEntering)
        {
            doorTimer = 0;
            return;
        }

        controller.UpdateHitbox();

        var de = controller.doorEntering;

        body.isKinematic = true;
        body.velocity = Vector2.zero;

        if (doorTimer < doorDuration / 2f && doorTimer + Time.fixedDeltaTime >= doorDuration / 2f)
        {
            transform.position = body.position =
                new Vector3(de.otherDoor.transform.position.x, de.otherDoor.transform.position.y, 1);
            animator.SetTrigger("door");
            if (de.otherDoor.isGoal)
            {
                GameManager.Instance.WinByGoal(controller);
                return;
            }

            controller.doorDirection = false;
            de.otherDoor.photonView.RPC(nameof(DoorManager.SomeoneEntered), RpcTarget.All, true);
            photonView.RPC("PlaySound", RpcTarget.All, Enums.Sounds.World_Door_Open);
            // controller.cameraController.Recenter();
        }
        else if (doorTimer < doorDuration / 4f && doorTimer + Time.fixedDeltaTime >= doorDuration / 4f)
        {
            de.photonView.RPC(nameof(DoorManager.SomeoneExited), RpcTarget.All);
            photonView.RPC("PlaySound", RpcTarget.All, Enums.Sounds.World_Door_Close);
            controller.fadeOut.FadeOutAndIn(true);
        }
        else if (doorTimer < doorDuration / 1.33f && doorTimer + Time.fixedDeltaTime >= doorDuration / 1.33f)
        {
            de.otherDoor.photonView.RPC(nameof(DoorManager.SomeoneExited), RpcTarget.All);
            photonView.RPC("PlaySound", RpcTarget.All, Enums.Sounds.World_Door_Close);
        }

        if (doorTimer >= doorDuration)
        {
            controller.doorEntering = null;
            body.isKinematic = false;
            controller.onGround = false;
            controller.properJump = false;
            controller.koyoteTime = 1;
            controller.crouching = false;
            controller.alreadyGroundpounded = true;
            controller.pipeTimer = 0.25f;
            body.velocity = Vector2.zero;
        }

        doorTimer += Time.fixedDeltaTime;
    }

    public void DisableAllModels()
    {
        smallModel.SetActive(false);
        largeModel.SetActive(false);
        blueShell.SetActive(false);
        propellerHelmet.SetActive(false);
        animator.avatar = smallAvatar;
    }
}