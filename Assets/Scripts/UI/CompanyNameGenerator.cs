using TMPro;
using UnityEngine;

public class CompanyNameGenerator : MonoBehaviour {
    public string companyName;
    [SerializeField] private TMP_Text label;

    void Start() {
        companyName = GenerateCompanyName();
        label.text = $"{companyName}<space=-6><font=\"BiosFont\"><size=26><voffset=30>\u00ae";
    }

    private static string GenerateCompanyName() {
        var c = "N";
        c += Perso.GetItem(new[] {"i", "l"});
        c += Perso.GetItem(new[] {"n", "m"});
        c += Perso.GetItem(new[] {"i", "t", "l"});
        c += Perso.GetItem(new[] {"e", "a", "o"});
        c += Perso.GetItem(new[] {"n", "m"});
        c += c != "Ninten" ? "d" : "t";
        c += Perso.GetItem(new[] {"e", "a", "o"});
        return c;
    }
}