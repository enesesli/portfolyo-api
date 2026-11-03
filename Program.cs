using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddDbContext<AppDb>(options =>
    options.UseSqlite("Data Source=portfolyo.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapGet("/merhaba", () => "Merhaba, ben Enes'in API'si!");

app.MapGet("/merhaba/{ad}", (string ad) => $"Merhaba {ad}, hoş geldin!");

var projeler = new List<Proje>
{
    new Proje(
        "Otonom Drone Tespit ve Etkisiz Hale Getirme Simülasyonu",
        "Bitirme projesi. Unreal Engine ve Microsoft AirSim ile otonom drone simülasyonu; hedef takibi yapan kule, balistik hesaplamayla atış sistemi ve Python ile Unreal arasında UDP üzerinden gerçek zamanlı haberleşme.",
        new[] { "Unreal Engine", "C++", "Python" }),
    new Proje(
        "Satış Siparişi Kontrol Ekranı (Cockpit)",
        "Çift panelli (ağaç + liste) etkileşimli sipariş takip ekranı. Excel'den veri aktarımı, arka plan işleri ve sipariş başlık/kalem seviyesinde güncelleme özellikleri.",
        new[] { "SAP ABAP" }),
    new Proje(
        "PDF Form Üretimi ve Otomatik E-posta",
        "SAP içinde Adobe Forms ile PDF belge üretimi ve oluşan belgenin ilgili kişilere otomatik e-posta ile gönderilmesi.",
        new[] { "SAP ABAP" }),
    new Proje(
        "Dinamik Tablo Editörü",
        "Farklı tabloların anahtar alanlarını dinamik olarak tanıyıp düzenlemeye izin veren genel amaçlı editör. Yapılan her değişikliği kayıt altına alan loglama mekanizması içeriyor.",
        new[] { "SAP ABAP" }),
    new Proje(
        "Kişisel Portfolyo Sitesi",
        "Şu an baktığın site. Teknolojiye göre proje filtreleme, iletişim formu ve mobil uyumlu tasarım. GitHub ile Vercel üzerinden otomatik yayınlanıyor.",
        new[] { "React", "JavaScript", "CSS" }),
};

app.MapGet("/api/projeler", () => projeler);

app.MapPost("/api/mesajlar", async (IletisimMesaji yeni, AppDb db) =>
{
    if (string.IsNullOrWhiteSpace(yeni.Ad) ||
        string.IsNullOrWhiteSpace(yeni.Eposta) ||
        string.IsNullOrWhiteSpace(yeni.Mesaj))
    {
        return Results.BadRequest("Tüm alanlar zorunludur.");
    }

    db.Mesajlar.Add(new KayitliMesaj
    {
        Ad = yeni.Ad,
        Eposta = yeni.Eposta,
        Mesaj = yeni.Mesaj,
        Tarih = DateTime.Now
    });
    await db.SaveChangesAsync();

    return Results.Ok("Mesaj alındı.");
});

app.MapGet("/api/mesajlar", async (AppDb db) =>
    await db.Mesajlar.OrderByDescending(m => m.Tarih).ToListAsync());

app.Run();

record Proje(string Baslik, string Aciklama, string[] Teknolojiler);
record IletisimMesaji(string Ad, string Eposta, string Mesaj);

class KayitliMesaj
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string Eposta { get; set; } = "";
    public string Mesaj { get; set; } = "";
    public DateTime Tarih { get; set; }
}

class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }
    public DbSet<KayitliMesaj> Mesajlar => Set<KayitliMesaj>();
}