using IranConnect.Application.Common.Interfaces;
using IranConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IranConnect.Infrastructure.BackgroundServices;

// One-shot seeder. Populates the IranianApps table from the original built-in
// list the first time the table is empty. After that, the catalog is managed
// entirely by admins via the CRUD endpoints — the seeder never overwrites
// existing rows.
public class IranianAppSeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IranianAppSeeder> _logger;

    public IranianAppSeeder(
        IServiceScopeFactory scopeFactory,
        ILogger<IranianAppSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // Free plan: only these packages are usable without Premium.
    private static readonly HashSet<string> FreePackages = new()
    {
        "ir.easytrader.orbis.m.twa",
        "com.samanpr.blu",
        "ir.mci.ecareapp"
    };

    private static readonly (string Pkg, string En, string Fa)[] Apps =
    {
        ("com.samanpr.blu", "Blu Bank", "بلوبانک"),
        ("com.samanpr.blujr", "Blu Junior Bank", "بلو جونیوز"),
        ("ir.easytrader.orbis.m.twa", "EasyTrader", "ایزی‌تریدر"),
        ("com.emofid.rnmofid", "Mofid Mobile", "کارگزاری مفید"),
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
        ("ir.zypod.app", "Zypod", "زیپاد"),
        ("com.isc.bsinew", "Bank Saderat", "بانک صادرات"),
        ("com.sibche.aspardproject.app", "Sibche", "سیبچه"),
        ("com.nar.bimito", "Bimito", "بیمیتو"),
        ("com.bimebazar.bimebazar", "Bime Bazar", "بیمه بازار"),
        ("com.saman.singlewindow", "Saman Bank", "بانک سامان"),
        ("com.fam.fam", "Fam", "فام"),
        ("ir.sep.sesoot", "Sesoot", "سه‌سوت"),
        ("com.mydigipay.app.android", "DigiPay", "دیجی‌پی"),
        ("ir.omidbank", "Bank Omid", "امید بانک"),
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
        ("ir.stts.bjt", "Bajet", "باجت"),
        ("com.tosan.dara.sina", "Bank Sina", "بانک سینا"),
        ("mob.banking.android.pasargad", "Bank Pasargad", "بانک پاسارگاد"),
        ("com.tosan.dara.mehriran", "Mehrino", "مهرینو"),
        ("com.tosan.dara.saman", "Bank Saman", "بانک سامان"),
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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider
                .GetRequiredService<IApplicationDbContext>();

            if (await context.IranianApps.AnyAsync(cancellationToken))
            {
                _logger.LogInformation(
                    "IranianApps already seeded; skipping.");
                return;
            }

            foreach (var (pkg, en, fa) in Apps)
                context.IranianApps.Add(
                    IranianApp.Create(pkg, en, fa, FreePackages.Contains(pkg)));

            var count = await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Seeded {Count} Iranian apps into catalog.", count);
        }
        catch (Exception ex)
        {
            // Warn, do not crash startup (e.g. migration not applied yet).
            _logger.LogWarning(ex,
                "IranianApp seeding failed; catalog may be empty.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
