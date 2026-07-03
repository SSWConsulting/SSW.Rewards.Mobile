namespace SSW.Rewards.Application.System.Commands.Common;

public sealed record DemoPerson(
    string Key,
    string Name,
    string? Title,
    bool IsStaff,
    double Activity,
    string[] Skills,
    string? Twitter = null,
    string? GitHub = null)
{
    public string Email => $"{Key}@{DemoDataSet.SeedEmailDomain}";
}

public sealed record DemoEvent(
    string Slug,
    string Name,
    DateTime Date,
    int Value,
    Icons Icon,
    double Attendance);

public sealed record DemoQuizQuestion(string Text, string[] Answers, int CorrectIndex);

public sealed record DemoQuiz(
    string Slug,
    string Title,
    string Description,
    Icons Icon,
    bool IsArchived,
    bool SeedCompletions,
    DemoQuizQuestion[] Questions);

public sealed record DemoReward(
    string Slug,
    string Name,
    string Description,
    int Cost,
    RewardType Type,
    Icons Icon,
    bool IsOnboarding = false,
    bool IsCarousel = false);

/// <summary>
/// The declarative "Northwind Traders" demo dataset: fictional but real-sounding people,
/// a calendar of community events, quizzes and a rewards catalog. All natural keys are
/// prefixed "demo:" so the seeder can recognise its own rows on re-runs.
/// </summary>
public static class DemoDataSet
{
    public const string SeedEmailDomain = "northwindtraders.example";
    public const string CompanyName = "Northwind Traders";
    public const string CompanyPlatformName = "Company";

    public const int StaffScanValue = 150;
    public const int UserScanValue = 100;
    public const int QuizValue = 500;

    public static readonly DemoPerson Flagship = new(
        "bob-northwind", "Bob Northwind", "Founder, Northwind Traders", IsStaff: true, Activity: 1.2,
        Skills: ["Leadership", "Azure", ".NET"], Twitter: "bobnorthwind", GitHub: "bobnorthwind");

    public static readonly DemoPerson[] Staff =
    [
        new("nancy-davolio", "Nancy Davolio", "Senior Software Engineer", true, 0.9, ["Angular", ".NET", "Azure"], Twitter: "nancydavolio"),
        new("andrew-fuller", "Andrew Fuller", "VP of Engineering", true, 0.7, ["Leadership", "Azure", "AI"], Twitter: "andrewfuller", GitHub: "andrewfuller"),
        new("janet-leverling", "Janet Leverling", "Solution Architect", true, 1.0, [".NET", "Clean Architecture", "Azure"], GitHub: "janetleverling"),
        new("margaret-peacock", "Margaret Peacock", "Principal Consultant", true, 0.8, ["Clean Architecture", "AI", "Leadership"]),
        new("steven-buchanan", "Steven Buchanan", "Engineering Manager", true, 0.6, ["Leadership", "DevOps"]),
        new("michael-suyama", "Michael Suyama", "Mobile Developer", true, 0.9, [".NET MAUI", "Flutter", ".NET"], GitHub: "msuyama"),
        new("robert-king", "Robert King", "DevOps Engineer", true, 0.8, ["DevOps", "Azure", "Docker"], GitHub: "robking"),
        new("laura-callahan", "Laura Callahan", "UX Designer", true, 0.7, ["UX Design", "Figma"]),
        new("anne-dodsworth", "Anne Dodsworth", "Graduate Developer", true, 1.0, ["React", ".NET"], Twitter: "annedods", GitHub: "annedodsworth"),
    ];

    // Community members — Northwind's classic customer contacts.
    public static readonly DemoPerson[] Community =
    [
        new("maria-anders", "Maria Anders", null, false, 0.9, ["Angular"], Twitter: "maria_anders"),
        new("ana-trujillo", "Ana Trujillo", null, false, 0.4, []),
        new("antonio-moreno", "Antonio Moreno", null, false, 0.7, [".NET"], GitHub: "antoniomoreno"),
        new("thomas-hardy", "Thomas Hardy", null, false, 0.8, ["Azure"], Twitter: "thardy"),
        new("christina-berglund", "Christina Berglund", null, false, 0.6, ["React"]),
        new("hanna-moos", "Hanna Moos", null, false, 0.5, [], GitHub: "hannamoos"),
        new("frederique-citeaux", "Frédérique Citeaux", null, false, 0.3, []),
        new("martin-sommer", "Martín Sommer", null, false, 0.4, []),
        new("laurence-lebihan", "Laurence Lebihan", null, false, 0.6, [".NET"]),
        new("elizabeth-lincoln", "Elizabeth Lincoln", null, false, 0.7, ["AI"], Twitter: "elizlincoln"),
        new("victoria-ashworth", "Victoria Ashworth", null, false, 0.8, ["Angular", "UX Design"]),
        new("patricio-simpson", "Patricio Simpson", null, false, 0.9, ["Flutter"], GitHub: "patriciosimpson"),
        new("francisco-chang", "Francisco Chang", null, false, 0.5, []),
        new("yang-wang", "Yang Wang", null, false, 0.6, ["AI", "Python"]),
        new("pedro-afonso", "Pedro Afonso", null, false, 0.7, ["DevOps"]),
        new("elizabeth-brown", "Elizabeth Brown", null, false, 0.3, []),
        new("sven-ottlieb", "Sven Ottlieb", null, false, 0.5, ["Docker"], GitHub: "svenottlieb"),
        new("janine-labrune", "Janine Labrune", null, false, 0.6, ["UX Design"]),
        new("ann-devon", "Ann Devon", null, false, 0.4, []),
        new("roland-mendel", "Roland Mendel", null, false, 0.5, [".NET"]),
        new("aria-cruz", "Aria Cruz", null, false, 1.0, ["React", "AI"], Twitter: "ariacruzdev", GitHub: "ariacruz"),
        new("diego-roel", "Diego Roel", null, false, 0.6, []),
        new("martine-rance", "Martine Rancé", null, false, 0.3, []),
        new("maria-larsson", "Maria Larsson", null, false, 0.5, ["Azure"]),
        new("peter-franken", "Peter Franken", null, false, 0.7, [".NET", "Docker"], GitHub: "peterfranken"),
        new("carine-schmitt", "Carine Schmitt", null, false, 0.6, ["Angular"]),
        new("paolo-accorti", "Paolo Accorti", null, false, 0.4, []),
        new("lino-rodriguez", "Lino Rodriguez", null, false, 0.5, []),
        new("eduardo-saavedra", "Eduardo Saavedra", null, false, 0.8, ["Flutter", "React"], Twitter: "edusaavedra"),
        new("jose-pedro-freyre", "José Pedro Freyre", null, false, 0.4, []),
        new("andre-fonseca", "André Fonseca", null, false, 0.7, ["Python", "AI"], GitHub: "andrefonseca"),
        new("howard-snyder", "Howard Snyder", null, false, 0.6, [".NET"]),
        new("manuel-pereira", "Manuel Pereira", null, false, 0.5, ["DevOps"]),
        new("mario-pontes", "Mario Pontes", null, false, 0.3, []),
        new("carlos-hernandez", "Carlos Hernández", null, false, 0.9, ["React", "Docker"], GitHub: "carloshdz"),
        new("yoshi-latimer", "Yoshi Latimer", null, false, 0.7, ["UX Design", "Figma"], Twitter: "yoshilatimer"),
        new("patricia-mckenna", "Patricia McKenna", null, false, 0.6, ["Angular"]),
        new("helen-bennett", "Helen Bennett", null, false, 0.4, []),
        new("philip-cramer", "Philip Cramer", null, false, 0.5, [".NET", "Clean Architecture"], GitHub: "philipcramer"),
        new("daniel-tonini", "Daniel Tonini", null, false, 0.6, ["Azure"]),
        new("annette-roulet", "Annette Roulet", null, false, 0.5, []),
        new("yoshi-tannamuri", "Yoshi Tannamuri", null, false, 0.4, ["Python"]),
        new("john-steel", "John Steel", null, false, 0.7, ["Leadership"]),
        new("renate-messner", "Renate Messner", null, false, 0.5, ["UX Design"]),
        new("jaime-yorres", "Jaime Yorres", null, false, 0.6, ["React"], Twitter: "jaimeyorres"),
        new("carlos-gonzalez", "Carlos González", null, false, 0.4, []),
        new("fran-wilson", "Fran Wilson", null, false, 0.8, ["AI", "Python"], Twitter: "franwilsondev", GitHub: "franwilson"),
        new("giovanni-rovelli", "Giovanni Rovelli", null, false, 0.3, []),
        new("catherine-dewey", "Catherine Dewey", null, false, 0.7, ["Angular", "UX Design"]),
        new("simon-crowther", "Simon Crowther", null, false, 0.9, [".NET", "React"], GitHub: "simoncrowther"),
    ];

    public static IEnumerable<DemoPerson> Everyone => Staff.Prepend(Flagship).Concat(Community);

    public static readonly DemoQuiz[] Quizzes =
    [
        new("dotnet-modern", ".NET Modern Practices", "Test your knowledge of modern .NET.", Icons.Lightbulb, IsArchived: false, SeedCompletions: false,
        [
            new("Which .NET feature reduces startup allocations by compiling ahead of time?", ["Native AOT", "JIT tiering", "ReadyToRun profiles", "Roslyn analyzers"], 0),
            new("What does `IAsyncEnumerable<T>` enable?", ["Streaming async iteration", "Parallel LINQ", "Sync-over-async", "Channel multiplexing"], 0),
            new("Which project type is best for cross-platform mobile in .NET?", [".NET MAUI", "WinForms", "WPF", "Blazor Server"], 0),
            new("What is the purpose of `record` types?", ["Value-based equality and immutability", "Faster reflection", "Database mapping", "Source generation"], 0),
        ]),
        new("ai-fundamentals", "AI Fundamentals", "From tokens to transformers — the basics.", Icons.Lightning, IsArchived: false, SeedCompletions: false,
        [
            new("What does a token represent to a large language model?", ["A chunk of text", "A neural layer", "A GPU core", "A database row"], 0),
            new("What is RAG?", ["Retrieval-augmented generation", "Random access generation", "Recursive agent graph", "Regularised attention gating"], 0),
            new("Which technique grounds an LLM in your own data?", ["Embeddings + vector search", "Bigger batch size", "Lower temperature", "Prompt caching"], 0),
            new("What does 'temperature' control?", ["Output randomness", "Context length", "Model size", "Token cost"], 0),
        ]),
        new("clean-architecture", "Clean Architecture Essentials", "Layers, dependencies and boundaries.", Icons.Puzzle, IsArchived: true, SeedCompletions: true,
        [
            new("Which layer holds enterprise business rules?", ["Domain", "Infrastructure", "Presentation", "Persistence"], 0),
            new("Dependencies should point…", ["Inwards, toward the domain", "Outwards, toward the UI", "Both ways", "Nowhere"], 0),
            new("Where do EF Core migrations belong?", ["Infrastructure", "Domain", "Application", "WebAPI"], 0),
            new("What pattern decouples use cases from controllers?", ["CQRS with MediatR", "Singleton", "Service locator", "Lazy loading"], 0),
        ]),
        new("northwind-history", "Northwind Trivia", "How well do you know the world's most famous sample company?", Icons.Trophy, IsArchived: true, SeedCompletions: true,
        [
            new("What does Northwind Traders sell?", ["Specialty food products", "Software licences", "Bicycles", "Office furniture"], 0),
            new("Which database shipped Northwind as a sample?", ["SQL Server / Access", "Oracle", "MongoDB", "SQLite"], 0),
            new("Who is Northwind's most famous customer persona?", ["Bob Northwind", "Clippy", "Tux", "Mona"], 0),
            new("Northwind's employee table famously has how many employees?", ["9", "42", "100", "3"], 0),
        ]),
    ];

    public static readonly DemoReward[] Rewards =
    [
        new("keepcup", "Northwind KeepCup", "Reusable branded coffee cup.", 4000, RewardType.Physical, Icons.Gift),
        new("smart-band", "Smart Fitness Band", "Track your steps between scans.", 5000, RewardType.Physical, Icons.Gift, IsCarousel: true),
        new("devcon-ticket", "Northwind DevCon Ticket", "One-day pass to the annual conference.", 2000, RewardType.Digital, Icons.CalendarCheck, IsCarousel: true),
        new("coffee-voucher", "Coffee Voucher", "A barista-made coffee on us.", 1000, RewardType.Digital, Icons.Gift),
        new("northwind-cap", "Northwind Cap", "Embroidered classic cap.", 1500, RewardType.Physical, Icons.Gift),
        new("sticker-pack", "Sticker Pack", "Laptop stickers for your first points.", 500, RewardType.Physical, Icons.Gift, IsOnboarding: true),
        new("arcade-night", "Team Arcade Night", "An evening pass for two at the retro arcade.", 3000, RewardType.Digital, Icons.Trophy),
    ];

    /// <summary>
    /// Generates the deterministic event calendar between two dates (inclusive):
    /// an annual conference, monthly user groups, quarterly hack days and two
    /// workshops a year. Dates are anchored to the calendar so re-runs are stable.
    /// </summary>
    public static IEnumerable<DemoEvent> GetEvents(DateTime from, DateTime to) =>
        EnumerateEvents(from.Year, to.Year).Where(e => e.Date >= from.Date && e.Date <= to.Date);

    private static IEnumerable<DemoEvent> EnumerateEvents(int fromYear, int toYear)
    {
        for (int year = fromYear; year <= toYear; year++)
        {
            var devCon = NthWeekday(year, 9, DayOfWeek.Tuesday, 3);
            yield return new($"devcon-{year}", $"Northwind DevCon {year}", devCon, 500, Icons.Lightning, 0.65);

            for (int month = 1; month <= 12; month++)
            {
                var ug = NthWeekday(year, month, DayOfWeek.Wednesday, 1);
                yield return new($"ug-{year}-{month:00}", $"Northwind User Group — {ug:MMMM yyyy}", ug, 200, Icons.Puzzle, 0.35);
            }

            foreach (var month in new[] { 2, 5, 8, 11 })
            {
                var hd = NthWeekday(year, month, DayOfWeek.Saturday, 1);
                yield return new($"hackday-{year}-{month:00}", $"Northwind Hack Day — {hd:MMMM yyyy}", hd, 200, Icons.Lightbulb, 0.25);
            }

            yield return new($"workshop-{year}-ca", $"Clean Architecture Workshop {year}", NthWeekday(year, 3, DayOfWeek.Thursday, 2), 300, Icons.Certificate, 0.20);
            yield return new($"workshop-{year}-ai", $"AI Hands-on Workshop {year}", NthWeekday(year, 10, DayOfWeek.Thursday, 2), 300, Icons.Certificate, 0.20);
        }
    }

    private static DateTime NthWeekday(int year, int month, DayOfWeek day, int n)
    {
        var date = new DateTime(year, month, 1);
        int offset = ((int)day - (int)date.DayOfWeek + 7) % 7;
        return date.AddDays(offset + 7 * (n - 1));
    }
}
