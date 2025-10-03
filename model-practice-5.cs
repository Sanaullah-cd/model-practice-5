using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Module05_Advanced
{
    // ========================
    // 🔒 SINGLETON – LOGGER
    // ========================

    public enum LogLevel { INFO, WARNING, ERROR }

    public class LoggerConfig
    {
        public string LogFilePath { get; set; } = "advanced_logs.txt";
        public LogLevel Level { get; set; } = LogLevel.INFO;
    }

    public sealed class Logger
    {
        private static Logger _instance;
        private static readonly object _lock = new object();
        private static LoggerConfig _config = new LoggerConfig();

        private Logger() { }

        public static Logger GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new Logger();
                }
            }
            return _instance;
        }

        public void LoadConfig(string configPath)
        {
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                _config = JsonSerializer.Deserialize<LoggerConfig>(json);
            }
        }

        public void SetLogLevel(LogLevel level) => _config.Level = level;

        public void Log(string message, LogLevel level)
        {
            if (level < _config.Level) return;

            lock (_lock)
            {
                string logMessage = $"{DateTime.Now:HH:mm:ss} [{level}] {message}";
                File.AppendAllText(_config.LogFilePath, logMessage + Environment.NewLine);
                Console.WriteLine(logMessage);
            }
        }
    }

    public class LogReader
    {
        private string _filePath;
        public LogReader(string filePath) => _filePath = filePath;

        public void ReadLogs(LogLevel? filter = null)
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine("No logs found.");
                return;
            }

            foreach (var line in File.ReadAllLines(_filePath))
            {
                if (filter == null || line.Contains($"[{filter}]"))
                    Console.WriteLine(line);
            }
        }
    }

    // ========================
    // 🏗 BUILDER – REPORTS
    // ========================

    public class ReportStyle
    {
        public string BackgroundColor { get; set; } = "White";
        public string FontColor { get; set; } = "Black";
        public int FontSize { get; set; } = 12;
    }

    public class Report
    {
        public string Header { get; set; }
        public string Content { get; set; }
        public string Footer { get; set; }
        public List<(string, string)> Sections { get; set; } = new List<(string, string)>();
        public ReportStyle Style { get; set; }

        public void Export(string format)
        {
            Console.WriteLine($"\n--- Exporting {format} Report ---");
            Console.WriteLine($"Header: {Header}");
            Console.WriteLine($"Content: {Content}");
            foreach (var sec in Sections)
                Console.WriteLine($"Section: {sec.Item1} -> {sec.Item2}");
            Console.WriteLine($"Footer: {Footer}");
            Console.WriteLine($"Style: BG={Style.BackgroundColor}, Font={Style.FontColor}, Size={Style.FontSize}");
        }
    }

    public interface IReportBuilder
    {
        void SetHeader(string header);
        void SetContent(string content);
        void SetFooter(string footer);
        void AddSection(string sectionName, string sectionContent);
        void SetStyle(ReportStyle style);
        Report GetReport();
    }

    public class TextReportBuilder : IReportBuilder
    {
        private Report _report = new Report();
        public void SetHeader(string header) => _report.Header = header;
        public void SetContent(string content) => _report.Content = content;
        public void SetFooter(string footer) => _report.Footer = footer;
        public void AddSection(string name, string content) => _report.Sections.Add((name, content));
        public void SetStyle(ReportStyle style) => _report.Style = style;
        public Report GetReport() => _report;
    }

    public class HtmlReportBuilder : IReportBuilder
    {
        private Report _report = new Report();
        public void SetHeader(string header) => _report.Header = $"<h1>{header}</h1>";
        public void SetContent(string content) => _report.Content = $"<p>{content}</p>";
        public void SetFooter(string footer) => _report.Footer = $"<footer>{footer}</footer>";
        public void AddSection(string name, string content) => _report.Sections.Add(($"<h2>{name}</h2>", $"<p>{content}</p>"));
        public void SetStyle(ReportStyle style) => _report.Style = style;
        public Report GetReport() => _report;
    }

    // Заглушка для PDF
    public class PdfReportBuilder : IReportBuilder
    {
        private Report _report = new Report();
        public void SetHeader(string header) => _report.Header = header;
        public void SetContent(string content) => _report.Content = content;
        public void SetFooter(string footer) => _report.Footer = footer;
        public void AddSection(string name, string content) => _report.Sections.Add((name, content));
        public void SetStyle(ReportStyle style) => _report.Style = style;
        public Report GetReport() => _report;
    }

    public class ReportDirector
    {
        public void ConstructReport(IReportBuilder builder, ReportStyle style)
        {
            builder.SetHeader("Monthly Report");
            builder.SetContent("Main data and analysis...");
            builder.AddSection("Sales", "Sales grew by 10%");
            builder.AddSection("Expenses", "Expenses stable");
            builder.SetFooter("Generated on " + DateTime.Now);
            builder.SetStyle(style);
        }
    }

    // ========================
    // 🌀 PROTOTYPE – GAME CHARACTERS
    // ========================

    public interface IPrototype<T>
    {
        T Clone();
    }

    public class Weapon : IPrototype<Weapon>
    {
        public string Name { get; set; }
        public int Damage { get; set; }
        public Weapon Clone() => new Weapon { Name = this.Name, Damage = this.Damage };
    }

    public class Armor : IPrototype<Armor>
    {
        public string Name { get; set; }
        public int Defense { get; set; }
        public Armor Clone() => new Armor { Name = this.Name, Defense = this.Defense };
    }

    public class Skill : IPrototype<Skill>
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Skill Clone() => new Skill { Name = this.Name, Type = this.Type };
    }

    public class Character : IPrototype<Character>
    {
        public string Name { get; set; }
        public int Health { get; set; }
        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Intelligence { get; set; }

        public Weapon Weapon { get; set; }
        public Armor Armor { get; set; }
        public List<Skill> Skills { get; set; } = new List<Skill>();

        public Character Clone()
        {
            Character copy = (Character)this.MemberwiseClone();
            copy.Weapon = Weapon?.Clone();
            copy.Armor = Armor?.Clone();
            copy.Skills = new List<Skill>();
            foreach (var s in Skills) copy.Skills.Add(s.Clone());
            return copy;
        }

        public override string ToString()
        {
            string skills = string.Join(", ", Skills.ConvertAll(s => s.Name));
            return $"{Name}: HP={Health}, STR={Strength}, AGI={Agility}, INT={Intelligence}, Weapon={Weapon?.Name}, Armor={Armor?.Name}, Skills={skills}";
        }
    }

    // ========================
    // MAIN
    // ========================
    class Program
    {
        static void Main()
        {
            // 🔒 Singleton demo
            Console.WriteLine("=== SINGLETON LOGGER ===");
            var logger = Logger.GetInstance();
            logger.LoadConfig("loggerConfig.json"); // JSON with { "LogFilePath": "advanced_logs.txt", "Level": "INFO" }

            Parallel.For(0, 5, i =>
            {
                var log = Logger.GetInstance();
                log.Log($"Message {i}", LogLevel.INFO);
            });

            var reader = new LogReader("advanced_logs.txt");
            reader.ReadLogs(LogLevel.INFO);

            // 🏗 Builder demo
            Console.WriteLine("\n=== REPORT BUILDER ===");
            ReportDirector director = new ReportDirector();
            var textBuilder = new TextReportBuilder();
            var htmlBuilder = new HtmlReportBuilder();

            director.ConstructReport(textBuilder, new ReportStyle { BackgroundColor = "Gray", FontColor = "Blue", FontSize = 14 });
            director.ConstructReport(htmlBuilder, new ReportStyle { BackgroundColor = "White", FontColor = "Black", FontSize = 12 });

            textBuilder.GetReport().Export("TEXT");
            htmlBuilder.GetReport().Export("HTML");

            // 🌀 Prototype demo
            Console.WriteLine("\n=== PROTOTYPE GAME CHARACTERS ===");
            Character warrior = new Character
            {
                Name = "Warrior",
                Health = 100,
                Strength = 20,
                Agility = 10,
                Intelligence = 5,
                Weapon = new Weapon { Name = "Sword", Damage = 15 },
                Armor = new Armor { Name = "Shield", Defense = 10 },
                Skills = new List<Skill> { new Skill { Name = "Slash", Type = "Physical" } }
            };

            Character mage = warrior.Clone();
            mage.Name = "Mage";
            mage.Intelligence = 25;
            mage.Weapon = new Weapon { Name = "Staff", Damage = 10 };
            mage.Skills.Add(new Skill { Name = "Fireball", Type = "Magic" });

            Console.WriteLine(warrior);
            Console.WriteLine(mage);
        }
    }
}
