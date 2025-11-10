const AudioStreamPlugin =
{
    ctx: null,
    sampleRate: 44100,
    initDelay: 0.5,
    scheduledTime: 0.0,
    volume: 1.0,
    volumeMultiplier: 1.0,

    Init: function(sampleRate, initDelay)
    {
        if (this.ctx) return;

        this.ctx = new window.AudioContext();
        this.sampleRate = sampleRate;
        this.initDelay = initDelay;

        console.log("Init: ", sampleRate, initDelay);
    },

    Play: function(offset, size) {
        if (!this.ctx) return;

        const arrayF32 = new Float32Array(size);
        for (let i = 0; i < size; ++i) {
            arrayF32[i] = HEAPF32[(offset >> 2) + i];
        }

        const gain = this.ctx.createGain();
        if (isFinite(this.volume) && isFinite(this.volumeMultiplier)) {
            gain.gain.value = this.volume * this.volumeMultiplier;
        }
        gain.connect(this.ctx.destination);

        const buf = this.ctx.createBuffer(1, size, sampleRate);
        buf.getChannelData(0).set(arrayF32);

        const src = this.ctx.createBufferSource();
        src.buffer = buf;
        src.connect(gain);

        const t = this.ctx.currentTime;
        if (t < this.scheduledTime) {
            this.scheduledTime += buf.duration;
        } else {
            this.scheduledTime = t + buf.duration + initDelay;
        }

        src.start(this.scheduledTime);

        // console.log("Playyyyyy", this.scheduledTime);
    },

    SetVolume: function(volume) {
        this.volume = volume;
    },
    
    SetVolumeMultiplier: function(volumeMultiplier) {
        this.volumeMultiplier = volumeMultiplier;
    }
};

mergeInto(LibraryManager.library, AudioStreamPlugin);
