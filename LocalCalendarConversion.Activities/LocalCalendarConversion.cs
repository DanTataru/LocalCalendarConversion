using System;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Data;
using System.IO;

namespace LocalCalendarConversion
{

    public class FromChristianCalendarYear : CodeActivity
    {
        [Category("Input")]
        [RequiredArgument]
        public InArgument<DateTime> ChristianCalendarYear { get; set; }

        [Category("Output")]
        public OutArgument<String> LocalCalenderYear { get; set; }

        [Category("Option")]
        [Description("ggyy年MM月dd日")]
        public InArgument<string> Format { get; set; }


        [Category("Option")]
        public InArgument<DataTable> LocalCalenderMetadata { get; set; }

        protected override void Execute(CodeActivityContext context)
        {


            DataTable est = LocalCalenderMetadata.Get(context);

            try
            {
                int i = est.Rows.Count;
            }
            catch
            {
                est = new DataTable("EraStartTable");
                est.Columns.Add("StartDate");
                est.Columns.Add("Era");

                DataRow heisei = est.NewRow();
                heisei["StartDate"] = "19890108";
                heisei["Era"] = "平成";
                est.Rows.Add(heisei);

                DataRow syouwa = est.NewRow();
                syouwa["StartDate"] = "19261225";
                syouwa["Era"] = "昭和";
                est.Rows.Add(syouwa);

                DataRow taishou = est.NewRow();
                taishou["StartDate"] = "19120730";
                taishou["Era"] = "大正";
                est.Rows.Add(taishou);

            }

            //もらった日付データをIntに直す＆分解
            //取得した日付をString型とint型に変換
            DateTime get_dateTime = ChristianCalendarYear.Get(context);
            String dateTime_str = get_dateTime.ToString("yyyyMMdd");
            int dateTime_int = int.Parse(dateTime_str);
            //yyyy部分を取得
            string AD_str = dateTime_str.Substring(0, 4);
            int AD_int = int.Parse(AD_str);

            int AD_only_diff = 0;
            int date_diff = -1;
            string eraName = null;
            string format = Format.Get(context);

            foreach (DataRow dr in est.Rows)
            {

                if (0 <= dateTime_int - int.Parse(dr["StartDate"].ToString()))
                {

                    //date_diffに値があるか確認する
                    if (date_diff == -1)
                    {
                        date_diff = dateTime_int - int.Parse(dr["StartDate"].ToString());
                        //yyyy部分のみで差分を取り、
                        AD_only_diff = AD_int - int.Parse(dr["StartDate"].ToString().Substring(0, 4));
                        //対応するEraをeraNameに代入
                        eraName = dr["Era"].ToString();


                    }

                    if (date_diff > dateTime_int - int.Parse(dr["StartDate"].ToString()))
                    {
                        //取得した日付と各年号のStartDateの差分を取りdate_diffの値を更新
                        date_diff = dateTime_int - int.Parse(dr["StartDate"].ToString());
                        //yyyy部分のみで差分を取り、
                        AD_only_diff = AD_int - int.Parse(dr["StartDate"].ToString().Substring(0, 4));
                        //対応するEraをeraNameに代入
                        eraName = dr["Era"].ToString();
                    }
                }


            }

            //date_diffの値が-1だった場合、エラー処理を施す
            if (date_diff == -1)
            {
                throw new ArgumentOutOfRangeException("Out of range");
            }


            if (string.IsNullOrEmpty(format))
            {
                //元号yy年mm月dd日の形にする
                dateTime_str = eraName + (AD_only_diff + 1).ToString() + "年" + get_dateTime.Month + "月" + get_dateTime.Day + "日";

            }
            else
            {
                dateTime_str = format
                     .Replace("gg", eraName)
                     .Replace("yy", (AD_only_diff + 1).ToString())
                     .Replace("MM", get_dateTime.Month.ToString())
                     .Replace("dd", get_dateTime.Day.ToString())

                     .Replace("g", "")
                     .Replace("y", "")
                     .Replace("M", "")
                     .Replace("d", "");



            }

            LocalCalenderYear.Set(context, dateTime_str);

        }
    }

    public class GetLocalCalenderMetadata : CodeActivity
    {

        [Category("Input")]
        [RequiredArgument]
        public InArgument<String> OutputFolderPath { get; set; }



        protected override void Execute(CodeActivityContext context)
        {


            DataTable dt = new DataTable("EraStartTable");
            dt.Columns.Add("StartDate");
            dt.Columns.Add("Era");

            DataRow heisei = dt.NewRow();
            heisei["StartDate"] = "19890108";
            heisei["Era"] = "平成";
            dt.Rows.Add(heisei);

            DataRow syouwa = dt.NewRow();
            syouwa["StartDate"] = "19261225";
            syouwa["Era"] = "昭和";
            dt.Rows.Add(syouwa);

            DataRow taishou = dt.NewRow();
            taishou["StartDate"] = "19120730";
            taishou["Era"] = "大正";
            dt.Rows.Add(taishou);

            String fileName = "LocalCalenderMetadata.csv";
            String filePath = System.IO.Path.Combine(@OutputFolderPath.Get(context), @fileName);

            String separator = ",";
            String quote = "";
            String replace = "";
            int rows = dt.Rows.Count;
            int cols = dt.Columns.Count;
            string text;
            //保存用のファイルを開く。上書きモードで。
            StreamWriter writer = new StreamWriter(filePath, false, Encoding.GetEncoding("shift_jis"));


            //カラム名を保存する場合
            for (int i = 0; i < cols; i++)
            {
                //カラム名を取得
                if (quote != "")
                {
                    text = dt.Columns[i].ColumnName.Replace(quote, replace);
                }
                else
                {
                    text = dt.Columns[i].ColumnName;
                }
                if (i != cols - 1)
                {
                    writer.Write(quote + text + quote + separator);
                }
                else
                {
                    writer.WriteLine(quote + text + quote);
                }
            }

            //データの保存処理
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (quote != "")
                    {
                        text = dt.Rows[i][j].ToString().Replace(quote, replace);
                    }
                    else
                    {
                        text = dt.Rows[i][j].ToString();
                    }
                    if (j != cols - 1)
                    {
                        writer.Write(quote + text + quote + separator);
                    }
                    else
                    {
                        writer.WriteLine(quote + text + quote);
                    }
                }
            }
            //ストリームを閉じる
            writer.Close();


        }
    }

    public class ToChristianCalendarYear : CodeActivity
    {
        [Category("Output")]
        [RequiredArgument]
        public OutArgument<DateTime> ChristianCalendarYear { get; set; }

        [Category("Input")]
        public InArgument<String> LocalCalenderYear { get; set; }

        [Category("Option")]
        public InArgument<DataTable> LocalCalenderMetadata { get; set; }

        protected override void Execute(CodeActivityContext context)
        {


            DataTable est = LocalCalenderMetadata.Get(context);

            try
            {
                int i = est.Rows.Count;
            }
            catch
            {
                est = new DataTable("EraStartTable");
                est.Columns.Add("StartDate");
                est.Columns.Add("Era");

                DataRow heisei = est.NewRow();
                heisei["StartDate"] = "19890108";
                heisei["Era"] = "平成";
                est.Rows.Add(heisei);

                DataRow syouwa = est.NewRow();
                syouwa["StartDate"] = "19261225";
                syouwa["Era"] = "昭和";
                est.Rows.Add(syouwa);

                DataRow taishou = est.NewRow();
                taishou["StartDate"] = "19120730";
                taishou["Era"] = "大正";
                est.Rows.Add(taishou);

            }

            var localCalenderYear = LocalCalenderYear.Get(context);
            var localCalenderYearArray = localCalenderYear.Split('/');

            string inputEra;
            string localYear;
            string localMonth;
            string localDay;

            try
            {
                inputEra = localCalenderYearArray[0];
                localYear = localCalenderYearArray[1];
                localMonth = localCalenderYearArray[2];
                localDay = localCalenderYearArray[3];
            }
            catch
            {
                throw new FormatException("An available format is only gg/yy/MM/dd.");
            }

            var christianYear = -1;

            foreach (DataRow estRow in est.Rows)
            {
                if (estRow["Era"].ToString() == inputEra)
                {
                    christianYear = int.Parse(localYear) + int.Parse(estRow["StartDate"].ToString().Substring(0, 4)) - 1;
                    ChristianCalendarYear.Set(context, new DateTime(christianYear, int.Parse(localMonth), int.Parse(localDay)));
                }
            }

            if (christianYear == -1)
            {
                throw new ArgumentOutOfRangeException(String.Format("{0} is an unknown era in LocalCalenderMetadata.", inputEra));
            }
        }

    }

}
