using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using WindowsFormsApp1.Database;

namespace WindowsFormsApp1
{
    public class 내보내기<TModel> where TModel : Models.ModelBase
    {
        private static DatabaseContext db = 데이터베이스.접근;

        public static void DB로(TModel model) => 데이터베이스.추가(model);

        public static void 엑셀로(TModel model, string 파일경로) {
            using (OleDbConnection conn = new OleDbConnection(GetConnectionString(파일경로))) {
                OleDbCommand command = null;
                // Excel 파일 준비
                if (!File.Exists(파일경로)) {
                    //Excel file에 Column 정보 전달
                    command = 새테이블생성(model, conn);
                    command.ExecuteNonQuery();
                }

                var Column이름목록 = Column정보생성(model);
                Type 테이블타입 = typeof(TModel);
                string 데이터삽입Command = $"INSERT INTO ({String.Join(",", Column이름목록)}) VALUES ({{0}})";
                List<string> Column데이터 = new List<string>();
                foreach (var Column정보 in Column이름목록.Select(이름 => 테이블타입.GetProperty(이름)))
                {
                    if (Column정보.PropertyType == typeof(int) || Column정보.PropertyType == typeof(decimal))
                        Column데이터.Add($"\"{Column정보.GetValue(model)}\"");
                    else if (Column정보.PropertyType == typeof(DateTime)) {
                        DateTime value = (DateTime)Column정보.GetValue(model);
                        Column데이터.Add($"\"{value.ToString()}\""); // FormatString 확인 필요
                    }
                    else
                        Column데이터.Add($"\"{Column정보.GetValue(model).ToString()}\"");
                }
                command = new OleDbCommand(데이터삽입Command, conn);
                command.ExecuteNonQuery();
            }

        }
        private static OleDbCommand 새테이블생성(TModel model, OleDbConnection conn) {
            var Column이름목록 = Column정보생성(model);
            Type 테이블타입 = typeof(TModel);

            List<string> Payload_Column정보 = new List<string>();

            foreach (var Column정보 in Column이름목록.Select(이름 => 테이블타입.GetProperty(이름))) {
                if (Column정보.PropertyType == typeof(decimal))
                    Payload_Column정보.Add($"{Column정보.Name} DECIMAl");
                else if (Column정보.PropertyType == typeof(int))
                    Payload_Column정보.Add($"{Column정보.Name} INT");
                else if (Column정보.PropertyType == typeof(DateTime))
                    Payload_Column정보.Add($"{Column정보.Name} DATETIME");
                else
                    Payload_Column정보.Add($"{Column정보.Name} TEXT");
            }
            var command = conn.CreateCommand();
            command.CommandText = $"CREATE TABLE Sheet1 ({String.Join(", ", Payload_Column정보)});";
            return command;
        }
        private static string GetConnectionString(string 파일경로) 
            => 파일경로.EndsWith("xlsx") 
                ? $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={파일경로};Extended Properties=\"Excel 8.0;HDR=YES;\""
                : $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={파일경로};Extended Properties=\"Excel 12.0;HDR=YES;\"";
        
        public static IEnumerable<string> Column정보생성(TModel model) 
            => typeof(TModel).GetProperties(System.Reflection.BindingFlags.Public).Select(prop => prop.Name);

        //private static DataTable 불러오기_Excel(string 파일경로, OleDbConnection conn) {
        //    string connectionString = GetConnectionString(파일경로);

        //    DataTable result = new DataTable();

        //    if (conn.State != ConnectionState.Open)
        //        conn.Open();
        //    OleDbDataAdapter adapter = new OleDbDataAdapter("select * from [Sheet1$]", conn);
        //    adapter.Fill(result);

        //    return result;
        //}
        //private static DataRow 데이터정제_Excel용(TModel model) {
        //    var Column이름목록 = Column정보생성(model);

        //    DataTable dt = new DataTable();
        //    dt.Columns.AddRange(Column이름목록.Select(name => new DataColumn(name)).ToArray());
        //    DataRow newRow = dt.NewRow();

        //    foreach (var Column이름 in Column이름목록)
        //    {
        //        newRow[Column이름] = typeof(TModel).GetProperty(Column이름).GetValue(model);
        //    }
        //    return newRow;
        //}
    }
}
