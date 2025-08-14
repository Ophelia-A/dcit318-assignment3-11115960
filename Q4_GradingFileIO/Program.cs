using System;
using System.Collections.Generic;
using System.IO;

namespace GradingSystem
{
    // a) Student class
    public class Student
    {
        public int Id;
        public string FullName;
        public int Score;

        public Student(int id, string fullName, int score)
        {
            Id = id;
            FullName = fullName;
            Score = score;
        }

        public string GetGrade()
        {
            if (Score >= 80 && Score <= 100) return "A";
            if (Score >= 70 && Score <= 79)  return "B";
            if (Score >= 60 && Score <= 69)  return "C";
            if (Score >= 50 && Score <= 59)  return "D";
            return "F";
        }

        public override string ToString() =>
            $"{FullName} (ID: {Id}): Score = {Score}, Grade = {GetGrade()}";
    }

    // b) & c) Custom exceptions
    public class InvalidScoreFormatException : Exception
    {
        public InvalidScoreFormatException(string message) : base(message) { }
    }

    public class MissingFieldException : Exception
    {
        public MissingFieldException(string message) : base(message) { }
    }

    // d) Processor class
    public class StudentResultProcessor
    {
        // Reads CSV-like .txt: "Id,Full Name,Score"
        public List<Student> ReadStudentsFromFile(string inputFilePath)
        {
            var students = new List<Student>();

            using (var reader = new StreamReader(inputFilePath))
            {
                string? line;
                int lineNo = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    lineNo++;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length < 3)
                        throw new MissingFieldException(
                            $"Line {lineNo}: expected 3 fields (Id, FullName, Score) but got {parts.Length}.");

                    // Trim fields
                    var idRaw = parts[0].Trim();
                    var nameRaw = parts[1].Trim();
                    var scoreRaw = parts[2].Trim();

                    if (string.IsNullOrWhiteSpace(idRaw) ||
                        string.IsNullOrWhiteSpace(nameRaw) ||
                        string.IsNullOrWhiteSpace(scoreRaw))
                    {
                        throw new MissingFieldException($"Line {lineNo}: one or more fields are empty.");
                    }

                    if (!int.TryParse(idRaw, out int id))
                        throw new InvalidScoreFormatException($"Line {lineNo}: Id '{idRaw}' is not a valid integer.");

                    if (!int.TryParse(scoreRaw, out int score))
                        throw new InvalidScoreFormatException($"Line {lineNo}: Score '{scoreRaw}' is not a valid integer.");

                    students.Add(new Student(id, nameRaw, score));
                }
            }

            return students;
        }

        public void WriteReportToFile(List<Student> students, string outputFilePath)
        {
            using (var writer = new StreamWriter(outputFilePath))
            {
                foreach (var s in students)
                {
                    writer.WriteLine($"{s.FullName} (ID: {s.Id}): Score = {s.Score}, Grade = {s.GetGrade()}");
                }
            }
        }
    }

    public static class Program
    {
        public static void Main()
        {
            // We look for files in the app's output folder (bin/.../Q4_GradingFileIO/)
            string inputPath  = Path.Combine(AppContext.BaseDirectory, "input.txt");
            string outputPath = Path.Combine(AppContext.BaseDirectory, "report.txt");

            try
            {
                var processor = new StudentResultProcessor();

                // e-ii/iii
                var students = processor.ReadStudentsFromFile(inputPath);
                processor.WriteReportToFile(students, outputPath);

                Console.WriteLine("Report generated successfully.");
                Console.WriteLine($"Input : {inputPath}");
                Console.WriteLine($"Output: {outputPath}");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Error: Input file not found. Make sure 'input.txt' is placed next to the executable.");
            }
            catch (InvalidScoreFormatException ex)
            {
                Console.WriteLine($"Invalid score format: {ex.Message}");
            }
            catch (MissingFieldException ex)
            {
                Console.WriteLine($"Missing field: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }
    }
}
