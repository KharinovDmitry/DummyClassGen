using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DummyClassGen
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] input = File.ReadAllLines(@"./input.txt");
            List<ClassModel> classModels = ToClassModels(input);
            CreateFiles(classModels);
            Console.ReadKey();
        }

        private static void CreateFiles(List<ClassModel> classModels)
        {
            foreach (var classModel in classModels)
                File.WriteAllText(classModel.Name + ".cs", GenerateCs(classModel));
        }

        private static List<ClassModel> ToClassModels(string[] input)
        {
            List<ClassModel> res = new List<ClassModel>();
            foreach (var item in Separate(input))
                if (item != null)
                    res.Add(Validate(item));
            return res;
        }

        static string GenerateCs(ClassModel classModel)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("namespace SillyProgram\n{");
            if (classModel.BaseClass != null)
                sb.AppendLine("\tclass " + classModel.Name + " : " + classModel.BaseClass);
            else
                sb.AppendLine("\tclass " + classModel.Name);
            sb.AppendLine("\t{");

            bool hasPrivate = false;
            foreach (var field in classModel.Fields)
            {
                if (field.Modifier == "private")
                {
                    sb.AppendLine("\t\t" + field.Modifier + " " + field.Type + " " + field.Name + ";");
                    hasPrivate = true;
                }
            }

            if (hasPrivate)
                sb.AppendLine();

            foreach (var field in classModel.Fields)
                if (field.Modifier == "public")
                    sb.AppendLine("\t\t" + field.Modifier + " " + field.Type + " " + field.Name + " { get; set; }");

            sb.AppendLine("\t}");
            sb.AppendLine("}");
            return sb.ToString();
        }
        static string[] Separate(string[] input)
        {
            string[] res = new string[(input.Length) + 1 / 3];
            for (int i = 0; i < (input.Length + 1) / 3; i++)
                res[i] = String.Join("\n", new string[] { input[i * 3], input[i * 3 + 1]});
            return res;
        }
        static ClassModel Validate(string input)
        {
            ClassModel res = new ClassModel();
            string classInfo = input.Split('\n')[0].Replace("Класс: ", "");
            string fieldsInfo = input.Split('\n')[1].Replace("Поля: ", "");
            string[] fields = fieldsInfo.Replace(", ", ",").Split(',');
            SetClassInfo(res, classInfo);

            res.Fields = new Field[fields.Length];
            for(int i = 0; i < fields.Length; i++)
            {
                Field newField = new Field();
                SetModifier(ref fields[i], newField);
                SetType(ref fields[i], newField);
                SetName(ref fields[i], newField);
                res.Fields[i] = newField;
            }

            return res;
        }

        private static void SetName(ref string field, Field newField)
        {
            if (newField.Modifier == "public")
                newField.Name = char.ToUpper(field[0]) + field.Substring(1);
            else if (newField.Modifier == "private")
                newField.Name = char.ToLower(field[0]) + field.Substring(1);
        }

        private static void SetType(ref string field, Field newField)
        {
            if (field.Contains("список"))
            {
                newField.Type = $"List<{GetType(field.Split(' ')[1])}>";
                field = field.Replace($"List<{field.Split(' ')[1]}> ", "");
            }
            else
            {
                newField.Type = GetType(field.Split(' ')[0]);
                field = field.Replace(field.Split(' ')[0] + " ", "");
            }
        }

        private static string GetType(string input)
        {
            switch (input)
            {
                case "число":
                    return "int";
                case "строка":
                    return "string";
                case "символ":
                    return "char";
                case "флаг":
                    return "bool";
                default:
                    return input;
            }
        }
        private static void SetModifier(ref string field, Field newField)
        {
            if (field.Contains("скрыт"))
            {
                newField.Modifier = "private";
                field = field.Replace("скрытый ", "");
                field = field.Replace("скрытая ", "");
                field = field.Replace("скрытое ", "");
            }
            else
            {
                newField.Modifier = "public";
            }
        }

        private static void SetClassInfo(ClassModel res, string classInfo)
        {
            if (classInfo.Contains("наследует"))
            {
                res.Name = classInfo.Split(' ')[0];
                res.BaseClass = classInfo.Split(' ')[2];
            }
            else
            {
                res.Name = classInfo;
            }
        }
    }
}
