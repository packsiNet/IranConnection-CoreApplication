using IranConnect.Application.Common.Interfaces;
using IranConnect.Domain.Enums;

namespace IranConnect.Infrastructure.Services;

public class IranianAppService : IIranianAppService
{
    // Free plan: only these 2 apps
    private static readonly List<IranianAppDto> FreeApps = new()
    {
        new("ir.bmi.bam.nativeweb", "Bank Melli", "بانک ملی"),
        new("com.samanpr.blujr", "Blu Bank", "بلوبانک")
    };

    private static readonly List<IranianAppDto> AllApps = new()
    {
        new("com.samanpr.blujr", "Blu Bank", "بلوبانک"),
        new("ir.easytrader.orbis.m.twa", "EasyTrader", "ایزی‌تریدر"),
        new("com.emofid.rnmofid", "Mofid Mobile", "موبایل مفید"),
        new("market.nobitex", "Nobitex", "نوبیتکس"),
        new("ir.mci.ecareapp", "Hamrahe Aval", "همراه اول"),
        new("ir.bmi.bam.nativeweb", "Bank Melli", "بانک ملی"),
        new("com.ada.mbank.mehr", "Bank Mehr Iran", "بانک مهر ایران"),
        new("ir.mobillet.app", "Mobilet Saman", "موبایلت سامان"),
        new("com.citydi.hplus", "City Plus", "سیتی پلاس"),
        new("com.parsmobapp", "Pars Mobile", "پارس موبایل"),
        new("ir.tejaratbank.tata.mobile.android.tejarat", "Bank Tejarat", "بانک تجارت"),
        new("ir.ayantech.ghabzino", "Ghabzino", "قبضینو"),
        new("com.refahbank.dpi.android", "Bank Refah", "بانک رفاه"),
        new("com.hamidrezabashiri.ezcard", "EzCard", "ای‌زی کارت"),
        new("com.mofidonline.mobile", "Mofid Online", "مفید آنلاین"),
        new("ir.zypod.app", "Zypod", "زیپود"),
        new("com.isc.bsinew", "Bank Saderat", "بانک صادرات"),
        new("com.sibche.aspardproject.app", "Aspard", "اسپرد"),
        new("com.nar.bimito", "Bimito", "بیمیتو"),
        new("com.bimebazar.bimebazar", "Bime Bazar", "بیمه بازار"),
        new("com.saman.singlewindow", "Saman Bank", "بانک سامان"),
        new("com.fam.fam", "Fam", "فام"),
        new("ir.sep.sesoot", "Sesoot", "سه‌سوت"),
        new("com.mydigipay.app.android", "DigiPay", "دیجی‌پی"),
        new("ir.omidbank", "Bank Omid", "بانک امید"),
        new("com.pmb.mobile", "Bank Maskan", "بانک مسکن"),
        new("com.tosan.dara.postbank", "Post Bank", "پست بانک"),
        new("digital.neobank", "Neo Bank", "نئوبانک"),
        new("com.dotin.wepod", "Wepod", "وپد"),
        new("com.bki.mobilebanking.android", "Bank Karafarin", "بانک کارآفرین"),
        new("co.nilin.faraznative", "Faraz", "فراز"),
        new("mob.banking.android.taavon", "Bank Taavon", "بانک توسعه تعاون"),
        new("ir.izbank.omnichannel", "IZ Bank", "ایران زمین"),
        new("mob.banking.android.resalat", "Bank Resalat", "بانک رسالت"),
        new("co.redbank.app", "Red Bank", "رد بانک"),
        new("com.tosan.dara.day", "Bank Day", "بانک دی"),
        new("com.melal", "Melal", "ملل"),
        new("app.sepino", "Sepino", "سپینو"),
        new("ir.stts.bjt", "Bank Jodat", "بانک جودت"),
        new("com.tosan.dara.sina", "Bank Sina", "بانک سینا"),
        new("mob.banking.android.pasargad", "Bank Pasargad", "بانک پاسارگاد"),
        new("com.tosan.dara.mehriran", "Bank Mehr Iran", "بانک مهر ایران"),
        new("ir.tes.sarmayeh", "Bank Sarmayeh", "بانک سرمایه"),
        new("ir.hafhashtad.android780", "Haf Hashtad", "هفت‌هشتاد"),
        new("com.sheypoor.mobile", "Sheypoor", "شیپور"),
        new("ir.divar", "Divar", "دیوار"),
        new("com.digikala", "Digikala", "دیجی‌کالا"),
        new("ir.basalam.app", "Basalam", "باسلام"),
        new("com.okala", "Okala", "اوکالا"),
        new("ir.torob", "Torob", "ترب"),
        new("com.myirancell", "Irancell", "ایرانسل"),
        new("ir.rightel.myrightel", "Rightel", "رایتل"),
        new("com.farsitel.bazaar", "Bazaar", "بازار"),
        new("ir.eitaa.messenger", "Eitaa", "ایتا"),
        new("app.rbmain.a", "Rubika", "روبیکا"),
        new("mobi.mmdt.ottplus", "OTT Plus", "اوتی‌تی پلاس")
    };

    public List<IranianAppDto> GetAllApps() => AllApps;

    public List<IranianAppDto> GetFreeApps() => FreeApps;

    public List<IranianAppDto> GetAllowedApps(SubscriptionPlan plan)
        => plan == SubscriptionPlan.Free ? FreeApps : AllApps;
}
