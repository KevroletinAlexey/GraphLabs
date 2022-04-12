using System.Security.Cryptography;
using Newtonsoft.Json;
using WebApplication2.Entity;
using WebApplication2.Entity.VariantData;


namespace WebApplication2.DAL;

 public class InitialData
    {
        private readonly PasswordHashCalculator _calculator;

        public InitialData(PasswordHashCalculator calculator)
        {
            _calculator = calculator;
        }
        
        public IEnumerable<TaskModule> GetTaskModules()
        {
            // taskModule #1
            yield return new TaskModule
            {
                Id = 1,
                Name = "Шаблон",
                Description = "Шаблон модуля.",
                Version = "2.0"
            };

            // taskModule #2
            yield return  new TaskModule
            {
                Id = 2,
                Name = "Внешняя устойчивость",
                Description = "Модуль Лизы",
                Version = "1.0"
            };
            
            // taskModule #3
            yield return  new TaskModule
            {
                Id = 3,
                Name = "Планарность",
                Description = "Модуль Насти",
                Version = "1.0"
            };
            
            // taskModule #4
            yield return  new TaskModule
            {
                Id = 4,
                Name = "Изоморфизм",
                Description = "Модуль Насти",
                Version = "1.0"
            };
            
            // taskModule #5
            yield return  new TaskModule
            {
                Id = 5,
                Name = "Построить граф по матрице смежности",
                Description = "Модуль Эллины",
                Version = "1.0"
            };
            
            // taskModule #6
            yield return  new TaskModule
            {
                Id = 6,
                Name = "Построить матрицу смежности по графу",
                Description = "Модуль Наташи",
                Version = "1.0"
            };
            
            // taskModule #7
            yield return  new TaskModule
            {
                Id = 7,
                Name = "Построить матрицу инцидентности по графу",
                Description = "Модуль Наташи",
                Version = "1.0"
            };
        }

        public IEnumerable<TaskVariant> GetTaskVariants(IEnumerable<TaskModule> modules)
        {
            var idCounter = 0;

            var sampleData = new VariantData<Graph>
            {
                Type = VariantDataType.Graph,
                Value = new Graph
                {
                    Vertices = new[] {"1", "2", "3", "4", "5"},
                    Edges = new[]
                    {
                        new Edge {Source = "1", Target = "2"},
                        new Edge {Source = "2", Target = "3"},
                        new Edge {Source = "3", Target = "4"},
                        new Edge {Source = "4", Target = "5"},
                        new Edge {Source = "5", Target = "1"}
                    }
                }
            };
            
            foreach (var taskModule in modules)
            {
                for (var i = 0; i < 3; i++)
                {
                    idCounter++;
                    yield return new TaskVariant
                    {
                        Id = idCounter,
                        Name = $"Вариант {idCounter}",
                        TaskModule = taskModule,
                        VariantData = JsonConvert.SerializeObject(new [] { sampleData })
                    };
                }
            }
        }

        public IEnumerable<User> GetUsers()
        {
            var crypto = new RNGCryptoServiceProvider();
            var salts = Enumerable.Range(0, 10)
                .Select(_ => new byte[16])
                .Select(s =>
                {
                    crypto.GetBytes(s);
                    return s;
                })
                .ToArray();
            
            var idCounter = 0;
            yield return new Teacher
            {
                Id = ++idCounter,
                Email = "admin@graphlabs.ru",
                FirstName = "Администратор",
                LastName = "Администратор",
                FatherName = "Администратор",
                PasswordSalt = salts[idCounter],
                PasswordHash = _calculator.Calculate("admin", salts[idCounter])
            };
            
            yield return new Student
            {
                Id = ++idCounter,
                Email = "student-1@graphlabs.ru",
                FirstName = "Студент Первый Тестовый",
                LastName = "Первый",
                FatherName = "Тестовый",
                Group = "Первая Тестовая",
                PasswordSalt = salts[idCounter],
                PasswordHash = _calculator.Calculate("первый", salts[idCounter])
            };
            
            yield return new Student
            {
                Id = ++idCounter,
                Email = "student-2@graphlabs.ru",
                FirstName = "Студент Второй Тестовый",
                LastName = "Второй",
                FatherName = "Тестовый",
                Group = "Первая Тестовая",
                PasswordSalt = salts[idCounter],
                PasswordHash = _calculator.Calculate("второй", salts[idCounter])
            };
            
            yield return new Student
            {
                Id = ++idCounter,
                Email = "student-3@graphlabs.ru",
                FirstName = "Студент Третий Тестовый",
                LastName = "Третий",
                FatherName = "Тестовый",
                Group = "Вторая Тестовая",
                PasswordSalt = salts[idCounter],
                PasswordHash = _calculator.Calculate("третий", salts[idCounter])
            };
            
            yield return new Student
            {
                Id = ++idCounter,
                Email = "student-4@graphlabs.ru",
                FirstName = "Студент Четвёртый Тестовый",
                LastName = "Четвёртый",
                FatherName = "Тестовый",
                Group = "Вторая Тестовая",
                PasswordSalt = salts[idCounter],
                PasswordHash = _calculator.Calculate("четвёртный", salts[idCounter])
            };
        }
    }
    