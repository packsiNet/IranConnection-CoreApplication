using IranConnect.Application.Common.Interfaces;

namespace IranConnect.Infrastructure.Services;

public class IranianAppService : IIranianAppService
{
    // Free plan: only these packages are accessible without Premium
    private static readonly HashSet<string> FreePackages = new()
    {
        "ir.bmi.bam.nativeweb", // Bank Melli
        "com.samanpr.blujr"     // Blu Bank
    };

    private static readonly List<(string Pkg, string En, string Fa)> Apps = new()
    {
        ("com.samanpr.blujr", "Blu Bank", "بلوبانک"),
        ("ir.easytrader.orbis.m.twa", "EasyTrader", "ایزی‌تریدر"),
        ("com.emofid.rnmofid", "Mofid Mobile", "موبایل مفید"),
        ("market.nobitex", "Nobitex", "نوبیتکس"),
        ("ir.mci.ecareapp", "Hamrahe Aval", "همراه اول"),
        ("ir.bmi.bam.nativeweb", "Bank Melli", "بانک ملی"),
        ("com.ada.mbank.mehr", "Bank Mehr Iran", "بانک مهر ایران"),
        ("ir.mobillet.app", "Mobilet Saman", "موبایلت سامان"),
        ("com.citydi.hplus", "City Plus", "سیتی پلاس"),
        ("com.parsmobapp", "Pars Mobile", "پارس موبایل"),
        ("ir.tejaratbank.tata.mobile.android.tejarat", "Bank Tejarat", "بانک تجارت"),
        ("ir.ayantech.ghabzino", "Ghabzino", "قبضینو"),
        ("com.refahbank.dpi.android", "Bank Refah", "بانک رفاه"),
        ("com.hamidrezabashiri.ezcard", "EzCard", "ای‌زی کارت"),
        ("com.mofidonline.mobile", "Mofid Online", "مفید آنلاین"),
        ("ir.zypod.app", "Zypod", "زیپود"),
        ("com.isc.bsinew", "Bank Saderat", "بانک صادرات"),
        ("com.sibche.aspardproject.app", "Aspard", "اسپرد"),
        ("com.nar.bimito", "Bimito", "بیمیتو"),
        ("com.bimebazar.bimebazar", "Bime Bazar", "بیمه بازار"),
        ("com.saman.singlewindow", "Saman Bank", "بانک سامان"),
        ("com.fam.fam", "Fam", "فام"),
        ("ir.sep.sesoot", "Sesoot", "سه‌سوت"),
        ("com.mydigipay.app.android", "DigiPay", "دیجی‌پی"),
        ("ir.omidbank", "Bank Omid", "بانک امید"),
        ("com.pmb.mobile", "Bank Maskan", "بانک مسکن"),
        ("com.tosan.dara.postbank", "Post Bank", "پست بانک"),
        ("digital.neobank", "Neo Bank", "نئوبانک"),
        ("com.dotin.wepod", "Wepod", "وپد"),
        ("com.bki.mobilebanking.android", "Bank Karafarin", "بانک کارآفرین"),
        ("co.nilin.faraznative", "Faraz", "فراز"),
        ("mob.banking.android.taavon", "Bank Taavon", "بانک توسعه تعاون"),
        ("ir.izbank.omnichannel", "IZ Bank", "ایران زمین"),
        ("mob.banking.android.resalat", "Bank Resalat", "بانک رسالت"),
        ("co.redbank.app", "Red Bank", "رد بانک"),
        ("com.tosan.dara.day", "Bank Day", "بانک دی"),
        ("com.melal", "Melal", "ملل"),
        ("app.sepino", "Sepino", "سپینو"),
        ("ir.stts.bjt", "Bank Jodat", "بانک جودت"),
        ("com.tosan.dara.sina", "Bank Sina", "بانک سینا"),
        ("mob.banking.android.pasargad", "Bank Pasargad", "بانک پاسارگاد"),
        ("com.tosan.dara.mehriran", "Bank Mehr Iran", "بانک مهر ایران"),
        ("ir.tes.sarmayeh", "Bank Sarmayeh", "بانک سرمایه"),
        ("ir.hafhashtad.android780", "Haf Hashtad", "هفت‌هشتاد"),
        ("com.sheypoor.mobile", "Sheypoor", "شیپور"),
        ("ir.divar", "Divar", "دیوار"),
        ("com.digikala", "Digikala", "دیجی‌کالا"),
        ("ir.basalam.app", "Basalam", "باسلام"),
        ("com.okala", "Okala", "اوکالا"),
        ("ir.torob", "Torob", "ترب"),
        ("com.myirancell", "Irancell", "ایرانسل"),
        ("ir.rightel.myrightel", "Rightel", "رایتل"),
        ("com.farsitel.bazaar", "Bazaar", "بازار"),
        ("ir.eitaa.messenger", "Eitaa", "ایتا"),
        ("app.rbmain.a", "Rubika", "روبیکا"),
        ("mobi.mmdt.ottplus", "OTT Plus", "اوتی‌تی پلاس")
    };

    private static readonly List<IranianAppDto> Catalog =
        Apps.Select(a => new IranianAppDto(
                a.Pkg, a.En, a.Fa, FreePackages.Contains(a.Pkg)))
            .ToList();

    public List<IranianAppDto> GetAppCatalog() => Catalog;
}
