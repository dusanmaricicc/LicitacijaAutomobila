/* 
   skripta za kreiranje baze podataka (MS SQL Server)
   
   
   Ova skripta:
     1) Kreira bazu LicitacijaAutomobilaDb
     2) Kreira tabele ASP.NET Core Identity mehanizma (uloge, korisnici, ...)
     3) Kreira domenske tabele aplikacije (Kategorije, Automobili, Ponude, ...)
     4) Ubacuje pocetne podatke (seed): uloge i kategorije

  */

IF DB_ID('LicitacijaAutomobilaDb') IS NULL
BEGIN
    CREATE DATABASE LicitacijaAutomobilaDb;
END
GO

USE LicitacijaAutomobilaDb;
GO




IF OBJECT_ID('dbo.AspNetRoles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetRoles (
        Id                 NVARCHAR(450)  NOT NULL PRIMARY KEY,
        Name               NVARCHAR(256)  NULL,
        NormalizedName     NVARCHAR(256)  NULL,
        ConcurrencyStamp   NVARCHAR(MAX)  NULL
    );
    CREATE UNIQUE INDEX RoleNameIndex ON dbo.AspNetRoles(NormalizedName) WHERE NormalizedName IS NOT NULL;
END
GO

IF OBJECT_ID('dbo.AspNetUsers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUsers (
        Id                   NVARCHAR(450)  NOT NULL PRIMARY KEY,
        -- dodatna polja definisana u ApplicationUser
        Ime                  NVARCHAR(50)   NOT NULL DEFAULT(''),
        Prezime              NVARCHAR(50)   NOT NULL DEFAULT(''),
        Adresa               NVARCHAR(200)  NULL,
        DatumRegistracije    DATETIME2      NOT NULL DEFAULT(SYSDATETIME()),
        Aktivan              BIT            NOT NULL DEFAULT(1),
        -- standardna Identity polja
        UserName             NVARCHAR(256)  NULL,
        NormalizedUserName   NVARCHAR(256)  NULL,
        Email                NVARCHAR(256)  NULL,
        NormalizedEmail      NVARCHAR(256)  NULL,
        EmailConfirmed       BIT            NOT NULL DEFAULT(0),
        PasswordHash         NVARCHAR(MAX)  NULL,
        SecurityStamp        NVARCHAR(MAX)  NULL,
        ConcurrencyStamp     NVARCHAR(MAX)  NULL,
        PhoneNumber          NVARCHAR(MAX)  NULL,
        PhoneNumberConfirmed BIT            NOT NULL DEFAULT(0),
        TwoFactorEnabled     BIT            NOT NULL DEFAULT(0),
        LockoutEnd           DATETIMEOFFSET NULL,
        LockoutEnabled       BIT            NOT NULL DEFAULT(1),
        AccessFailedCount    INT            NOT NULL DEFAULT(0)
    );
    CREATE UNIQUE INDEX UserNameIndex ON dbo.AspNetUsers(NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
    CREATE INDEX EmailIndex ON dbo.AspNetUsers(NormalizedEmail);
END
GO

IF OBJECT_ID('dbo.AspNetUserRoles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserRoles (
        UserId  NVARCHAR(450) NOT NULL,
        RoleId  NVARCHAR(450) NOT NULL,
        CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_AspNetUserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AspNetUserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.AspNetUserClaims', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserClaims (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        UserId      NVARCHAR(450) NOT NULL,
        ClaimType   NVARCHAR(MAX) NULL,
        ClaimValue  NVARCHAR(MAX) NULL,
        CONSTRAINT FK_AspNetUserClaims_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.AspNetUserLogins', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserLogins (
        LoginProvider       NVARCHAR(450) NOT NULL,
        ProviderKey          NVARCHAR(450) NOT NULL,
        ProviderDisplayName NVARCHAR(MAX) NULL,
        UserId               NVARCHAR(450) NOT NULL,
        CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
        CONSTRAINT FK_AspNetUserLogins_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.AspNetUserTokens', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserTokens (
        UserId  NVARCHAR(450) NOT NULL,
        LoginProvider NVARCHAR(450) NOT NULL,
        Name    NVARCHAR(450) NOT NULL,
        Value   NVARCHAR(MAX) NULL,
        CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
        CONSTRAINT FK_AspNetUserTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.AspNetRoleClaims', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetRoleClaims (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        RoleId      NVARCHAR(450) NOT NULL,
        ClaimType   NVARCHAR(MAX) NULL,
        ClaimValue  NVARCHAR(MAX) NULL,
        CONSTRAINT FK_AspNetRoleClaims_Roles FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
    );
END
GO



IF OBJECT_ID('dbo.Kategorije', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Kategorije (
        Id     INT IDENTITY(1,1) PRIMARY KEY,
        Naziv  NVARCHAR(50) NOT NULL
    );
END
GO

IF OBJECT_ID('dbo.Automobili', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Automobili (
        Id                 INT IDENTITY(1,1) PRIMARY KEY,
        Marka              NVARCHAR(30)     NOT NULL,
        Model              NVARCHAR(30)     NOT NULL,
        Godina             INT              NOT NULL,
        PocetnaCena        DECIMAL(18,2)    NOT NULL,
        Opis               NVARCHAR(1000)   NOT NULL,
        DatumPostavljanja  DATETIME2        NOT NULL DEFAULT(SYSDATETIME()),
        DatumPocetka       DATETIME2        NOT NULL,
        DatumZavrsetka     DATETIME2        NOT NULL,
        KategorijaId       INT              NOT NULL,
        ProdavacId         NVARCHAR(450)    NOT NULL,
        CONSTRAINT FK_Automobili_Kategorije FOREIGN KEY (KategorijaId) REFERENCES dbo.Kategorije(Id),
        CONSTRAINT FK_Automobili_Prodavac   FOREIGN KEY (ProdavacId)   REFERENCES dbo.AspNetUsers(Id)
    );
END
GO

IF OBJECT_ID('dbo.SlikeAutomobila', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SlikeAutomobila (
        Id             INT IDENTITY(1,1) PRIMARY KEY,
        PutanjaSlike   NVARCHAR(300) NOT NULL,
        AutomobilId    INT NOT NULL,
        CONSTRAINT FK_SlikeAutomobila_Automobili FOREIGN KEY (AutomobilId) REFERENCES dbo.Automobili(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.Ponude', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Ponude (
        Id            INT IDENTITY(1,1) PRIMARY KEY,
        Iznos         DECIMAL(18,2) NOT NULL,
        VremePonude   DATETIME2     NOT NULL DEFAULT(SYSDATETIME()),
        AutomobilId   INT           NOT NULL,
        KupacId       NVARCHAR(450) NOT NULL,
        CONSTRAINT FK_Ponude_Automobili FOREIGN KEY (AutomobilId) REFERENCES dbo.Automobili(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Ponude_Kupac      FOREIGN KEY (KupacId)     REFERENCES dbo.AspNetUsers(Id)
    );
END
GO

IF OBJECT_ID('dbo.Notifikacije', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifikacije (
        Id           INT IDENTITY(1,1) PRIMARY KEY,
        Poruka       NVARCHAR(500) NOT NULL,
        Datum        DATETIME2     NOT NULL DEFAULT(SYSDATETIME()),
        Procitano    BIT           NOT NULL DEFAULT(0),
        KorisnikId   NVARCHAR(450) NOT NULL,
        AutomobilId  INT NULL,
        CONSTRAINT FK_Notifikacije_Korisnik  FOREIGN KEY (KorisnikId)  REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Notifikacije_Automobil FOREIGN KEY (AutomobilId) REFERENCES dbo.Automobili(Id) ON DELETE SET NULL
    );
END
GO



-- Uloge
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = 'ADMINISTRATOR')
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Administrator', 'ADMINISTRATOR', NEWID());

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = 'PRODAVAC')
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Prodavac', 'PRODAVAC', NEWID());

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = 'KUPAC')
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Kupac', 'KUPAC', NEWID());
GO

-- Kategorije automobila
IF NOT EXISTS (SELECT 1 FROM dbo.Kategorije)
BEGIN
    INSERT INTO dbo.Kategorije (Naziv) VALUES
    (N'Limuzina'),
    (N'SUV'),
    (N'Hečbek'),
    (N'Karavan'),
    (N'Kabriolet'),
    (N'Terensko vozilo');
END
GO

/* Korisnicki nalozi (administrator, prodavac, kupac) i automobili
   se automatski kreiraju pri prvom pokretanju aplikacije  */
