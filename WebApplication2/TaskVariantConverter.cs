using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using WebApplication2.DAL;
using WebApplication2.Entity;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2;

public sealed class TaskVariantConverter
    {
        private readonly GraphLabsContext _db;

        private class VariantConvertException : Exception
        {
            public VariantConvertException(string error) : base(error)
            {
            }
            
            public VariantConvertException(string error, Exception inner) : base(error, inner)
            {
            }
        }

        public TaskVariantConverter(GraphLabsContext db)
        {
            _db = db;
        }
        
        public static Expression<Func<TaskVariant, string>> ToJsonExpression = 
            v => $@"{{""data"": {v.VariantData}, ""meta"": {{ ""name"": ""{v.Name}"", ""id"": ""{v.Id}"", ""moduleId"": ""{v.TaskModule.Id}"" }} }}";

        public static Func<TaskVariant, string> ToJson = ToJsonExpression.Compile();

        public async Task<TaskVariant> CreateOrUpdate(long id, string json)
        {
            var taskVariant = await _db.TaskVariants.SingleOrDefaultAsync(v => v.Id == id);
            if (taskVariant == null)
            {
                taskVariant = new TaskVariant();
            }
            
            var variant = TryExecute(() => JObject.Parse(json), "Не удалось распарсить данные.");
            var idFromJson = TryExecute(() => variant["meta"]["id"].Value<long>(), "Не удалось прочитать значение meta/id");
            if (idFromJson != taskVariant.Id)
            {
                throw new VariantConvertException(
                    "Не совпадает id в json (meta/id) и идентификатор реально обновляемого варианта.");
            }
            
            var moduleId = TryExecute(() => variant["meta"]["moduleId"].Value<long>(), "Не удалось прочитать значение meta/moduleId");
            if (taskVariant.TaskModule == null)
            {
                var module = await _db.TaskModules.SingleOrDefaultAsync(m => m.Id == moduleId);
                if (module == null)
                    throw new VariantConvertException($"Модуль с id={moduleId} не найден.");
                taskVariant.TaskModule = module;
            }
            else if (moduleId != taskVariant.TaskModule.Id)
            {
                throw new VariantConvertException(
                    "Значение moduleId в json (meta/moduleId) не совпадает с реальным идентификатором модуля");
            }

            var name = TryExecute(() => variant["meta"]["name"].Value<string>(), "Не удалось прочитать значение meta/name");
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new VariantConvertException("Имя варианта (meta/name) не может быть пустым");
            }
            taskVariant.Name = name;

            var dataArray = TryExecute(() => variant["data"], "Не удалось прочитать значение data");
            foreach (var data in dataArray.Children())
            {
                var type = TryExecute(() => data["type"].Value<string>(), "Не удалось прочитать значение data/type");
                if (string.IsNullOrWhiteSpace(type))
                {
                    throw new VariantConvertException("Тип варианта (data/type) не может быть пустым");
                }
            
                var value = TryExecute(() => data["value"], "Не удалось прочитать значение data/value");
                if (!value.Children().Any())
                {
                    throw new VariantConvertException("Значение варианта (data/value) не может быть пустым");
                }
            }

            taskVariant.VariantData = TryExecute(() => dataArray.ToString(), "Непредвиденная ошибка при записи VariantData");

            if (taskVariant.Id == 0)
                _db.Add(taskVariant);
            
            await _db.SaveChangesAsync();

            return taskVariant;
        }

        private static T TryExecute<T>(Func<T> f, string errorMessage)
        {
            try
            {
                return f();
            }
            catch (Exception e)
            {
                throw new VariantConvertException(errorMessage, e);
            }
        }
    }