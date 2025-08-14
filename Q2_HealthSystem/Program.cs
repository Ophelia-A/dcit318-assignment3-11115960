using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthSystem
{
    // a) Generic repository for entity management
    public class Repository<T>
    {
        private readonly List<T> items = new();

        // Fields: List<T> items
        // Methods:
        public void Add(T item) => items.Add(item);

        public List<T> GetAll() => new(items);

        public T? GetById(Func<T, bool> predicate) => items.FirstOrDefault(predicate);

        public bool Remove(Func<T, bool> predicate)
        {
            var toRemove = items.FirstOrDefault(predicate);
            if (toRemove is null) return false;
            return items.Remove(toRemove);
        }
    }

    // b) Patient class
    public class Patient
    {
        public int Id;
        public string Name;
        public int Age;
        public string Gender;

        public Patient(int id, string name, int age, string gender)
        {
            Id = id;
            Name = name;
            Age = age;
            Gender = gender;
        }

        public override string ToString() => $"Patient {{ Id={Id}, Name={Name}, Age={Age}, Gender={Gender} }}";
    }

    // c) Prescription class
    public class Prescription
    {
        public int Id;
        public int PatientId;
        public string MedicationName;
        public DateTime DateIssued;

        public Prescription(int id, int patientId, string medicationName, DateTime dateIssued)
        {
            Id = id;
            PatientId = patientId;
            MedicationName = medicationName;
            DateIssued = dateIssued;
        }

        public override string ToString() =>
            $"Prescription {{ Id={Id}, PatientId={PatientId}, Medication={MedicationName}, DateIssued={DateIssued:d} }}";
    }

    // g) HealthSystemApp
    public class HealthSystemApp
    {
        // Fields:
        private readonly Repository<Patient> _patientRepo = new();
        private readonly Repository<Prescription> _prescriptionRepo = new();
        private readonly Dictionary<int, List<Prescription>> _prescriptionMap = new();

        // Methods:
        public void SeedData()
        {
            // Add 2–3 Patients
            _patientRepo.Add(new Patient(1, "Kevin De Bruyne", 34, "Female"));
            _patientRepo.Add(new Patient(2, "Pep Guardiola", 60, "Male"));
            _patientRepo.Add(new Patient(3, "Khadija Shaw", 26, "Female"));

            // Add 4–5 Prescriptions with valid PatientIds
            _prescriptionRepo.Add(new Prescription(101, 1, "Amoxicillin 500mg", DateTime.Today.AddDays(-10)));
            _prescriptionRepo.Add(new Prescription(102, 1, "Paracetamol 1g", DateTime.Today.AddDays(-7)));
            _prescriptionRepo.Add(new Prescription(103, 2, "Ibuprofen 400mg", DateTime.Today.AddDays(-3)));
            _prescriptionRepo.Add(new Prescription(104, 2, "Cetirizine 10mg", DateTime.Today.AddDays(-1)));
            _prescriptionRepo.Add(new Prescription(105, 3, "Metformin 500mg", DateTime.Today));
        }

        public void BuildPrescriptionMap()
        {
            _prescriptionMap.Clear();
            foreach (var rx in _prescriptionRepo.GetAll())
            {
                if (!_prescriptionMap.TryGetValue(rx.PatientId, out var list))
                {
                    list = new List<Prescription>();
                    _prescriptionMap[rx.PatientId] = list;
                }
                list.Add(rx);
            }
        }

        public void PrintAllPatients()
        {
            Console.WriteLine("=== Patients ===");
            foreach (var p in _patientRepo.GetAll())
                Console.WriteLine(p);
        }

        public List<Prescription> GetPrescriptionsByPatientId(int patientId)
        {
            return _prescriptionMap.TryGetValue(patientId, out var list) ? new List<Prescription>(list) : new List<Prescription>();
        }

        public void PrintPrescriptionsForPatient(int id)
        {
            Console.WriteLine($"\n=== Prescriptions for PatientId {id} ===");
            var list = GetPrescriptionsByPatientId(id);
            if (list.Count == 0)
            {
                Console.WriteLine("No prescriptions found.");
                return;
            }
            foreach (var rx in list)
                Console.WriteLine(rx);
        }

        public void Run()
        {
            // Main Application Flow (i–v)
            SeedData();                 // ii
            BuildPrescriptionMap();     // iii
            PrintAllPatients();         // iv

            // v. Select one PatientId and display prescriptions
            var patientToShow = 2; // you can change to 1 or 3 to test
            PrintPrescriptionsForPatient(patientToShow);
        }
    }

    public static class Program
    {
        public static void Main()
        {
            var app = new HealthSystemApp();
            app.Run();
        }
    }
}
