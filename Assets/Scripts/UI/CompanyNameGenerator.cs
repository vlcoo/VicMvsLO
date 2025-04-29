using TMPro;
using UnityEngine;

public class CompanyNameGenerator : MonoBehaviour {
    public string companyName;
    [SerializeField] private TMP_Text label;

    void Start() {
        companyName = GenerateCompanyName();
        label.text = $"{companyName}<space=-6><font=\"BiosFont\"><size=40><voffset=28>\u00ae";
    }

    private static string GenerateCompanyName() {
        var c = "N";
        c += new[] {"i", "l"}[Random.Range(0, 2)];
        c += new[] {"n", "m"}[Random.Range(0, 2)];
        c += new[] {"i", "t", "l"}[Random.Range(0, 3)];
        c += new[] {"e", "a", "o"}[Random.Range(0, 3)];
        c += new[] {"n", "m"}[Random.Range(0, 2)];
        c += c != "Ninten" ? "d" : "t";
        c += new[] {"e", "a", "o"}[Random.Range(0, 3)];
        return c;
    }
}