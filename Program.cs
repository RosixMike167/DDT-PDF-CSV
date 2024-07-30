using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

Console.WriteLine("PDF Extractor by Michael Rosi!");
Console.Title = "PDF Extractor by Michael Rosi!";

start();
Console.WriteLine("Premi un pulsante per chiudere la schermata.");
Console.ReadKey();
Environment.Exit(0);

void start()
{
    string rootPath = AppContext.BaseDirectory;
    string dataPath = rootPath + @"\dati\";

    string[] args = Environment.GetCommandLineArgs()
        .Skip(1)
        .ToArray();
    FileInfo file = new FileInfo("null");
    string input = "";
    PdfDocument pdfDoc;
    PdfReader pdfReader;

    if (!Directory.Exists(dataPath))
    {
        Console.WriteLine("Creazine cartella: \"dati\"...");

        Directory.CreateDirectory(dataPath);
        Console.WriteLine("Cartella: \"dati\" creata.");
    }

    if (args.Length == 0)
    {
        Console.WriteLine("Incolla percorso file, oppure premere invio per prendere il file con l'ultima data.");
        Console.Write("> ");

        input = Console.ReadLine();
    } else
    {
        file = new FileInfo(args[0]);
        input = file.FullName;
        Console.WriteLine($"Applicazione avviata con file: {file.Name.ToUpper().Substring(0, file.Name.Length- 4)}.");

        if(!checkFile(file))
        {
            Console.Clear();
            Console.WriteLine($"ERRORE! Il file inserito: {file.Name} non e' un PDF!");
            return;
        }
    }

    if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
    {
        try
        {
            file = new DirectoryInfo(rootPath)
                .GetFiles("*.pdf")
                .OrderBy(p => p.CreationTimeUtc)
                .First();

            if (file == null || file.Name == "null")
            {
                Console.Clear();
                Console.WriteLine("Nessun file trovato.\n");
                start();
                return;
            }

            input = file.FullName;
            pdfReader = new PdfReader(file);
            pdfDoc = new PdfDocument(pdfReader);
        }
        catch (Exception)
        {
            Console.Clear();
            Console.WriteLine("Nessun file trovato.\n");
            start();
            return;
        }
    } else
    {
        try
        {
            if (!checkFile(file))
            {
                Console.Clear();
                Console.WriteLine($"ERRORE! Il file inserito: {file.Name} non e' un PDF!");
                return;
            }

            pdfReader = new PdfReader(input);
            pdfDoc = new PdfDocument(pdfReader);
        }
        catch (IOException)
        {
            Console.Clear();
            Console.WriteLine("Nessun file trovato.\n");
            start();
            return;
        }
    }

    Console.WriteLine($"Lettura file: {file.Name.ToUpper().Substring(0, file.Name.Length - 4)}");
    Console.WriteLine("+====================================+\n");

    int pgcount = pdfDoc.GetNumberOfPages();
    Dictionary<string, string> dict = new Dictionary<string, string>();

    using (StreamWriter outputFile = new StreamWriter(Path.Combine(dataPath, file.Name == "null" ? @"OUTPUT.csv" : file.Name.Substring(0, file.Name.Length - 4) + ".csv")))
    {
        for (int i = 1; i <= pgcount; i++)
        {
            Dictionary<string, string> map = read(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)));
            foreach (string key in map.Keys)
            {
                dict.Add(key, map[key]);
            }


            Console.WriteLine($"\nCreazione output CSV pagina: {i}/{pgcount}");
        }

        foreach (string key in dict.Keys)
        {
            outputFile.WriteLine(key + ";" + dict[key]);
        }

        outputFile.Flush();
        outputFile.Close();
    }

    Console.WriteLine("+====================================+\n");
}

Dictionary<string, string> read(string input)
{
    Dictionary<string, string> dictionary = new Dictionary<string, string>();

    string sectionPattern = @"NR\s*([\s\S]*?)(?=NR|$)";
    string numberPattern = @"(\d{13})[\s\S]*?(\d+)\s+(?=\d{1,2},\d{2})";

    MatchCollection sections = Regex.Matches(input, sectionPattern);

    foreach (Match sect in sections)
    {
        if (sect.Success)
        {
            string section = sect.Groups[1].Value;

            MatchCollection collection = Regex.Matches(section, numberPattern);

            foreach (Match match in collection)
            {
                if (match.Success)
                {
                    string ean = match.Groups[1].Value;
                    string qty = match.Groups[2].Value;

                    dictionary.Add(ean, qty);
                    Console.WriteLine($"isbn: {ean} - quantita': {qty} - isbn-7: {ean.Substring(6)}");
                }
            }
        }
    }

    return dictionary;
}

bool checkFile(FileInfo file)
{
    return file.Extension.Equals(".pdf");
}