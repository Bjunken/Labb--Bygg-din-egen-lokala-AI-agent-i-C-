# Labb: Bygg din egen lokala AI-agent i C#

## Introduktion

Välkommen till denna labb! Här ska du bygga din alldeles egna **AI-agent** som körs lokalt på din dator, skriven i C#. Du behöver ingen betald API-nyckel, och ingen internetanslutning krävs när agenten väl är igång — allt sker på din egen maskin. (Att installera Ollama och ladda ner själva språkmodellen kräver dock internet — se avsnittet Förkunskaper och förberedelser nedan.)

En enkel AI-agent kan beskrivas som ett program som tar emot instruktioner från en användare, skickar dem vidare till en språkmodell och använder modellens svar för att utföra eller presentera något. Det är ingen akademiskt fullständig definition, men den räcker gott för denna labb.

Mer konkret gör agenten du bygger tre saker:

1. Tar emot en fråga eller instruktion från en användare
2. Skickar den vidare till en språkmodell (AI:n som faktiskt "tänker")
3. Tar emot svaret och visar upp det

Det som gör en agent intressant — och det du övar mest på i denna labb — är att man kan **styra hur agenten beter sig** genom att sätta olika *parametrar*. Precis som du justerar reglagen på en stereo (volym, bas, diskant) för att ändra ljudet, kan du justera agentens parametrar för att ändra hur den svarar: hur kreativ den är, hur långa svaren blir, och vilken "personlighet" den har.

**Mål med labben:** Efter övningen ska du kunna:

- Sätta upp ett C#-konsolprojekt från grunden på ett strukturerat sätt
- Förklara vad parametrarna *system prompt*, *temperature* och *max tokens* gör
- Bygga en klass som skickar förfrågningar till en lokal AI-modell via HTTP
- Experimentera med parametrar och observera hur de påverkar resultatet
- Använda vedertagna C#-mönster som konfigurationsklasser, felhantering och tydlig kodstruktur

## Bakgrund: vad är en lokal AI-agent?

När man pratar om AI-agenter menar man ofta ett program som använder en **språkmodell** (Large Language Model, LLM) för att lösa uppgifter. "Lokal" betyder att själva språkmodellen körs på din egen dator istället för i molnet hos ett företag som OpenAI eller Anthropic. I denna labb använder vi ett gratisverktyg som heter **Ollama**, som gör det enkelt att köra öppna språkmodeller lokalt och prata med dem via ett vanligt HTTP-API — precis som vilket webb-API som helst.

Ditt C#-program pratar alltså inte direkt med "AI:n", utan skickar en HTTP-förfrågan till Ollama (som körs i bakgrunden på din dator), och Ollama skickar tillbaka ett svar.

### Vad är en "parameter"?

En parameter är en inställning som styr hur modellen genererar sitt svar. Tänk på det som reglage du kan vrida på innan du skickar en fråga. De viktigaste parametrarna du kommer jobba med i denna labb är:

| Parameter | Vad den gör | Exempel på värde |
| --- | --- | --- |
| `Model` | Vilken språkmodell som ska svara | `llama3.2` |
| `SystemPrompt` | Grundinstruktion som talar om för modellen vilken "roll" den har | "Du är en hjälpsam och kortfattad assistent." |
| `Temperature` | Hur kreativ/oförutsägbar modellen är. Lågt värde (t.ex. 0.1) ger fokuserade, förutsägbara svar. Högt värde (t.ex. 1.2) ger mer varierade och "kreativa" svar | `0.7` |
| `MaxTokens` | Ett ungefärligt tak för hur långt svaret får bli. Ett "token" är ungefär en ordbit — en förenkling, en token motsvarar inte alltid ett helt ord | `500` |
| `TopP` | Begränsar hur stort urval av möjliga nästa ord (tokens) modellen får välja bland. Ett lägre värde begränsar urvalet mer, medan 1.0 tillåter hela urvalet | `0.9` |

> Du behöver inte förstå exakt hur `TopP` fungerar matematiskt för att göra denna labb. Det viktiga är att experimentera och observera skillnaden — det gör du i Steg 5.

> **Varför är detta viktigt att lära sig?** Samma fråga kan ge helt olika svar beroende på vilka parametrar du satt. Att förstå och kunna styra dessa är en viktig del av att kunna påverka hur en AI-agent beter sig — kunskap som är lika relevant oavsett om du senare bygger vidare med molnbaserade API:er som OpenAI:s eller Anthropics.

## Förkunskaper och förberedelser

Du behöver inte kunna AI eller maskininlärning för denna labb — bara grundläggande C# (variabler, klasser, metoder). Innan du börjar, installera följande:

1. **.NET SDK** (version 8 eller senare) — ladda ner från [dotnet.microsoft.com](https://dotnet.microsoft.com/download). Kontrollera installationen genom att köra `dotnet --version` i en terminal.
2. **En kodeditor**, till exempel Visual Studio Code eller Visual Studio.
3. **Ollama** — ladda ner från [ollama.com](https://ollama.com) och installera. Detta är programmet som kör själva språkmodellen lokalt på din dator.
4. **En språkmodell** — när Ollama är installerat, öppna en terminal och skriv:

   ```
   ollama pull llama3.2
   ```

   Detta laddar ner en mindre, snabb modell som fungerar bra för denna labb (cirka 2 GB). Har du en äldre eller svagare dator kan du istället prova `ollama pull phi3`, som är ännu mindre.
5. **Testa att Ollama fungerar** genom att köra:

   ```
   ollama run llama3.2
   ```

   Om du får ett svar när du skriver en fråga fungerar allt som det ska. Skriv `/bye` för att avsluta testet — Ollama fortsätter ändå köra i bakgrunden och lyssnar på `http://localhost:11434`, vilket är adressen vårt C#-program kommer prata med.

> **Tips:** Ollama startar automatiskt en lokal server så fort programmet är installerat. Du behöver alltså inte hålla igång `ollama run` — det kommandot är bara till för att testa manuellt i terminalen.

## Steg 1: Skapa projektet

Öppna en terminal, navigera till en mapp där du vill spara ditt projekt och kör:

```
dotnet new console -n LokalAgent
cd LokalAgent
```

Detta skapar ett nytt konsolprojekt med standardmässig mappstruktur. Öppna mappen `LokalAgent` i din kodeditor. Du bör se följande:

```
LokalAgent/
├── LokalAgent.csproj
└── Program.cs
```

För att strukturera koden på ett bra sätt (god praxis!) kommer vi dela upp programmet i flera filer istället för att skriva allt i `Program.cs`. Skapa därför följande tomma filer i projektmappen redan nu:

```
AgentConfig.cs
LocalAgent.cs
```

**Varför delar vi upp koden?** Precis som du inte skulle skriva en hel uppsats som en enda lång mening, bör kod delas upp i tydliga, namngivna delar. `AgentConfig.cs` kommer hålla våra parametrar, `LocalAgent.cs` kommer hålla logiken för att prata med modellen, och `Program.cs` kommer vara vårt "startprogram" som knyter ihop allt.

## Steg 2: Skapa konfigurationsklassen för parametrarna

Här definierar vi själva parametrarna som ska styra agenten. Vi samlar dem i en egen klass — det gör dem enkla att hitta, ändra och experimentera med, istället för att värdena ligger utspridda i koden.

Öppna `AgentConfig.cs` och skriv:

```csharp
namespace LokalAgent;

/// <summary>
/// Samlar alla parametrar som styr hur agenten beter sig.
/// Genom att samla dem på ett ställe blir de enkla att hitta,
/// ändra och experimentera med.
/// </summary>
public class AgentConfig
{
    /// <summary>Vilken modell Ollama ska använda för att svara.</summary>
    public string Model { get; set; } = "llama3.2";

    /// <summary>
    /// Grundinstruktionen som talar om för modellen vilken roll
    /// eller "personlighet" den ska ha.
    /// </summary>
    public string SystemPrompt { get; set; } =
        "Du är en hjälpsam assistent som svarar kort och tydligt på svenska.";

    /// <summary>
    /// Styr hur kreativ/oförutsägbar modellen är.
    /// 0.0 = mycket fokuserat och förutsägbart.
    /// 1.0+ = mer varierat och "kreativt".
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>Ungefärligt tak för hur långt svaret får bli.</summary>
    public int MaxTokens { get; set; } = 500;

    /// <summary>
    /// Styr hur brett urval av ord modellen väljer bland.
    /// Lägre värde = mer begränsat, mer förutsägbart urval.
    /// </summary>
    public double TopP { get; set; } = 0.9;
}
```

**Vad händer här, rad för rad?**

- `namespace LokalAgent;` grupperar vår kod under ett gemensamt namn — god praxis i alla C#-projekt.
- `public class AgentConfig` skapar en egen datatyp som samlar alla parametrar. Att den är `public` betyder att andra filer i projektet kan använda den.
- Varje `{ get; set; }`-egenskap är en parameter du kan läsa och ändra.
- Standardvärdena (`= "llama3.2"`, `= 0.7` osv.) gör att du kan skapa en `AgentConfig` utan att behöva ange alla värden själv, men du kan alltid skriva över dem — det testar du i Steg 5.
- XML-kommentarerna (`///`) är standardpraxis för att dokumentera vad varje del av koden gör, vilket editorer visar som hjälptext.

> **För den nyfikne:** I modern C# finns även en variant som heter `record`, som ofta används för data som inte ska kunna ändras efter att den skapats (kallas *immutable*). I den här labben använder vi en vanlig `class` för att hålla fokus på själva AI-agenten.

## Steg 3: Bygg agentklassen

Nu bygger vi själva agenten — klassen som tar emot en fråga, skickar den till Ollama tillsammans med våra parametrar, och returnerar svaret.

### Vad händer när vi skickar en fråga?

Innan vi går in i koden, här är den övergripande bilden av vad som faktiskt händer:

```
Användaren skriver "Vad är C#?"
        ↓
   LocalAgent bygger en förfrågan (med dina parametrar)
        ↓
   Förfrågan skickas som JSON via HTTP till Ollama
        ↓
   Ollama skickar frågan vidare till språkmodellen
        ↓
   Språkmodellen genererar ett svar
        ↓
   Svaret skickas tillbaka som JSON
        ↓
   LocalAgent läser ut svarstexten
        ↓
   Svaret visas för användaren
```

### En snabb analogi: vad är HTTP?

Tänk på Ollama som en restaurang:

- Ditt C#-program är **kunden**
- HTTP-förfrågan är **beställningen**
- JSON är **språket beställningen skrivs på**
- Ollama är **köket**
- HTTP-svaret är **maten som kommer tillbaka**

Vårt C#-program skickar alltså en "beställning" till Ollamas API och väntar på ett svar — precis som när du beställer mat och väntar tills den är klar.

### Koden

Öppna `LocalAgent.cs` och skriv:

```csharp
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace LokalAgent;

/// <summary>
/// Pratar med en lokalt körande Ollama-server via HTTP och
/// skickar med de parametrar som definierats i AgentConfig.
/// </summary>
public class LocalAgent
{
    private readonly HttpClient _httpClient;
    private readonly AgentConfig _config;

    // Ollamas standardadress när det körs lokalt.
    private const string OllamaEndpoint = "http://localhost:11434/api/chat";

    public LocalAgent(AgentConfig config)
    {
        _config = config;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(2)
        };
    }

    /// <summary>
    /// Skickar en fråga till modellen och returnerar dess svar som text.
    /// </summary>
    public async Task<string> AskAsync(string userMessage)
    {
        var request = new OllamaRequest
        {
            Model = _config.Model,
            Stream = false,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = _config.SystemPrompt },
                new() { Role = "user", Content = userMessage }
            },
            Options = new OllamaOptions
            {
                Temperature = _config.Temperature,
                TopP = _config.TopP,
                NumPredict = _config.MaxTokens
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(OllamaEndpoint, request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();
            return result?.Message?.Content ?? "(Inget svar mottogs från modellen.)";
        }
        catch (HttpRequestException ex)
        {
            return $"Kunde inte nå Ollama. Kontrollera att Ollama körs. Felmeddelande: {ex.Message}";
        }
        catch (TaskCanceledException)
        {
            return "Förfrågan tog för lång tid (timeout). Prova en mindre modell eller lägre MaxTokens.";
        }
    }
}

// --- Klasser som beskriver formatet Ollamas API förväntar sig och skickar tillbaka ---
// God praxis: håll datamodellerna separata från själva logiken ovanför.

public class OllamaRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = new();

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("options")]
    public OllamaOptions Options { get; set; } = new();
}

public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}

public class OllamaOptions
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public double TopP { get; set; }

    [JsonPropertyName("num_predict")]
    public int NumPredict { get; set; }
}

public class OllamaResponse
{
    [JsonPropertyName("message")]
    public ChatMessage? Message { get; set; }
}
```

**Vad händer här?**

- Vi använder `HttpClient` — standardklassen i .NET för att göra HTTP-anrop — för att prata med Ollamas API.
- `AskAsync` är `async`. Nyckelordet `await` betyder ungefär: *"vänta på resultatet av detta arbete innan vi fortsätter här"*. Medan programmet väntar på att Ollama ska svara behöver datorn inte stå still och göra ingenting — det är därför `async`/`await` används så ofta när program pratar över nätverket.
- Konkret: när vi i Steg 4 skriver `string svar = await agent.AskAsync(input);` betyder det "skicka frågan till agenten och vänta tills agenten fått tillbaka svaret".
- Vi skickar med `_config.SystemPrompt`, `_config.Temperature`, `_config.TopP` och `_config.MaxTokens` i varje förfrågan — det är precis här dina parametrar från Steg 2 faktiskt används.
- `try/catch` runt anropet är god praxis: om Ollama inte körs, eller om något går fel, får användaren ett tydligt felmeddelande istället för att programmet kraschar.
- Klasserna längst ner (`OllamaRequest`, `ChatMessage` osv.) beskriver exakt vilket JSON-format Ollamas API vill ha. Till exempel motsvarar `OllamaRequest` ungefär denna JSON:

  ```json
  {
      "model": "llama3.2",
      "stream": false,
      "messages": [
          { "role": "user", "content": "Vad är C#?" }
      ]
  }
  ```

  När vi använder `PostAsJsonAsync` omvandlas C#-objektet automatiskt till JSON i det här formatet, som Ollama sedan kan läsa. Att separera dessa "datamodeller" från logiken i `LocalAgent` gör koden lättare att läsa och underhålla.

## Steg 4: Bygg huvudprogrammet

Innan vi skriver koden, en snabb påminnelse om varför vi delade upp projektet i tre filer:

```
AgentConfig.cs   → Inställningar (parametrarna)
LocalAgent.cs    → AI/HTTP-logik (pratar med Ollama)
Program.cs       → Användargränssnitt och programflöde
```

Öppna `Program.cs`, radera det som redan står där, och skriv:

```csharp
using LokalAgent;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("=== Din lokala AI-agent ===");
Console.WriteLine("Skriv en fråga och tryck Enter. Skriv 'avsluta' för att stänga programmet.\n");

// Här skapas parametrarna för agenten. Prova att ändra dessa värden
// i Steg 5 och observera hur svaren förändras!
var config = new AgentConfig
{
    Model = "llama3.2",
    SystemPrompt = "Du är en hjälpsam assistent som svarar kort och tydligt på svenska.",
    Temperature = 0.7,
    MaxTokens = 300,
    TopP = 0.9
};

var agent = new LocalAgent(config);

while (true)
{
    Console.Write("Du: ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    if (input.Trim().Equals("avsluta", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Agenten avslutas. Hej då!");
        break;
    }

    Console.WriteLine("Agenten tänker...");
    string svar = await agent.AskAsync(input);

    Console.WriteLine($"Agent: {svar}\n");
}
```

**Vad händer här?**

- `Console.OutputEncoding = System.Text.Encoding.UTF8;` ser till att svenska tecken som å, ä, ö visas korrekt i terminalen.
- Vi skapar en `AgentConfig` med tydliga, namngivna värden — det är mycket lättare att läsa och ändra än om parametrarna låg utspridda i koden.
- `while (true)` skapar en loop som håller programmet igång tills användaren skriver "avsluta" — ett vanligt mönster för enkla kommandoradsprogram.
- Notera `await agent.AskAsync(input)` — som du läste i Steg 3 betyder det "skicka frågan och vänta tills svaret kommit tillbaka". Toppnivåprogram i moderna .NET-projekt (som `Program.cs` här) tillåter `await` direkt utan extra kod runt omkring, vilket är standard sedan C# 9/.NET 6.

### Kör programmet

Säkerställ att Ollama körs i bakgrunden, gå sedan till projektmappen i terminalen och kör:

```
dotnet run
```

Om allt är korrekt konfigurerat bör du nu kunna skriva en fråga och få svar från din lokala agent!

> **Vad har du byggt?** Du har nu byggt ett C#-program som tar emot text från användaren, skapar ett objekt med konfiguration, bygger en HTTP-förfrågan, skickar JSON till ett lokalt API, tar emot JSON tillbaka, läser ut AI:ns svar och visar det i terminalen. Du har alltså kombinerat flera vanliga programmeringskoncept — objekt, HTTP, JSON, asynkron kod — i ett och samma projekt.

## Steg 5: Experimentera med parametrarna

Nu kommer den viktigaste delen av labben — att faktiskt förstå vad parametrarna gör genom att testa dem själv. Gör följande experiment, ett i taget. Ändra värdena i `config` i `Program.cs`, kör om programmet (`dotnet run`) och ställ **samma fråga** varje gång, till exempel: *"Berätta en kort historia om en katt."*

> **Viktigt att veta:** AI-modeller ger inte alltid exakt samma svar även om du använder samma fråga och samma inställningar. Leta därför efter generella skillnader mellan experimenten — förvänta dig inte identiska resultat varje gång.

Använd gärna denna tabell för att anteckna dina observationer under experimenten:

| Experiment | Värde | Din observation |
| --- | --- | --- |
| Temperature | 0.1 |  |
| Temperature | 1.3 |  |
| MaxTokens | 20 |  |
| MaxTokens | 500 |  |
| TopP | 0.1 |  |
| TopP | 1.0 |  |

### Experiment 1: Temperature

1. Sätt `Temperature = 0.1` och ställ frågan två gånger. Är svaren lika eller olika?
2. Sätt `Temperature = 1.3` och ställ samma fråga två gånger. Vad märker du nu?

*Förväntat resultat:* Med låg temperature bör svaren vara ganska lika varandra och "säkra". Med hög temperature bör svaren variera mer och kännas mer oväntade eller kreativa — men ibland även mindre sammanhängande.

### Experiment 2: SystemPrompt

Ändra `SystemPrompt` till exempelvis:

```csharp
SystemPrompt = "Du är en pirat som alltid svarar som en gammaldags sjörövare. Använd uttryck som 'arrr' och 'kapten'.";
```

Ställ samma fråga igen. Hur påverkar detta agentens "personlighet", även fast frågan är identisk?

### Experiment 3: MaxTokens

Sätt `MaxTokens = 20` och ställ en fråga som kräver ett längre svar, t.ex. *"Förklara hur fotosyntes fungerar."* Vad händer med svaret? Sätt sedan `MaxTokens = 500` och jämför.

### Experiment 4: TopP

Sätt `Temperature = 1.0` (så effekten syns tydligare) och testa `TopP = 0.1` respektive `TopP = 1.0` med samma fråga några gånger. Vilken skillnad märker du i hur varierade svaren är?

> **Tips för felsökning:** Om programmet inte får något svar, kontrollera att: (1) Ollama-programmet är igång, (2) du har laddat ner rätt modell med `ollama pull`, och (3) `Model`-värdet i din `AgentConfig` stavas exakt likadant som modellens namn i Ollama.

## Reflektionsfrågor (lämnas in tillsammans med koden)

Besvara följande frågor kort och konkret utifrån dina egna experiment. Observera att det inte finns ett enskilt facit på frågorna 4 och 5 — motivera utifrån vad du faktiskt observerade.

1. Vad hände med svaren när du sänkte respektive höjde `Temperature`? Ge ett konkret exempel från dina tester.
2. Varför tror du att `SystemPrompt` kan ändra agentens "personlighet" trots att själva frågan är identisk?
3. Vad hände när `MaxTokens` sattes till ett lågt värde? Varför tror du att modellen betedde sig som den gjorde?
4. Om du skulle bygga en agent för ett **kundtjänstsystem** där svaren måste vara konsekventa och sakliga, vilka ungefärliga värden skulle du välja på `Temperature` och `TopP`? Motivera utifrån dina observationer.
5. Om du istället skulle bygga en agent för att **brainstorma idéer till en berättelse**, hur skulle du ändra parametrarna, och varför?
6. Varför är det bra praxis att samla alla parametrar i en egen klass (`AgentConfig`) istället för att skriva in värdena direkt där de används i koden?

## Extra utmaningar (frivilligt)

Klar i förtid, eller sugen på mer? Prova någon av dessa:

- **Konversationshistorik:** Just nu glömmer agenten allt mellan varje fråga. Bygg om `LocalAgent` så att den sparar tidigare frågor och svar i en lista och skickar med hela historiken i varje anrop, så att agenten kan "minnas" konversationen.
- **Konfiguration via fil:** Flytta värdena i `AgentConfig` till en `appsettings.json`-fil istället för att hårdkoda dem i `Program.cs`. Detta är standardpraxis i verkliga .NET-projekt.
- **Flera fördefinierade personligheter:** Skapa flera färdiga `AgentConfig`-objekt (t.ex. "Formell assistent", "Kreativ författare", "Kodgranskare") som användaren kan välja mellan när programmet startar.
- **Streamat svar:** Ollamas API stödjer att skicka svaret bit för bit (`Stream = true`) istället för allt på en gång. Bygg om programmet så att svaret skrivs ut ord för ord medan det genereras, för en snabbare upplevd respons.
- **Enhetstester:** Skriv ett enkelt enhetstest (med t.ex. xUnit) som kontrollerar att `AgentConfig` verkligen använder rätt standardvärden om inget annat anges.
