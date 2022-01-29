using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using DataJuggler.PixelDatabase.Net;
using DataJuggler.PixelDatabase;
using AForge.Imaging;
using AForge;
using Image = System.Drawing.Image;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.FormModels;
using System.Drawing.Drawing2D;
using System.Media;



namespace WindowsFormsApp1
{

    partial class Form1
    {
        private Dictionary<string, 좌표> 좌표목록;
        List<카드정보> 엑시정보 = new List<카드정보> { };
        List<first5prices> prices = new List<first5prices>();
        private static ManualResetEvent mre = new ManualResetEvent(true);
        private const int 키입력지연 = 15;
        private const int 재등록가격차감 = 0;
        private bool is_roundcheck_running = false; CancellationTokenSource cts = new CancellationTokenSource(); // task 종료하는 토큰
        private bool newgame_checked = true;
        private bool restart_pressed = false;
        private bool 스위치 = true; bool 한영키변경완료 = false;
        int 현재ID = 0; ServerType 현재서버; string 현재위치 = ""; int 현재계정 = 11;
        private bool 가격지웠니 = false; private bool 첫옵션올스탯 = false;
        private bool is_setting_done = false; private bool is_sise_setting_done = false; bool is_pre_round_card_counted = true;
        int 순서 = 0; int energy = 0; int card = 0; int[] button = new int[12] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        int round = 0; int received_total = 0; int card_total = 24; int card_used = 0; int UsedCard_in_round = 0; int pre_round_card_cnt = 0;
        int pre_Class = 0; int pre_breed_min = 0; int pre_breed_max = 0; int pre_pure = 0; List<string> pre_parts = new List<string> { };
        bool toggle = false; //상대 카드갯수 세는데 쓰이는 토글 "상대카드갯수()"
        List<Bitmap> 이전출석부 = new List<Bitmap>();
        List<Bitmap> 이전사진첩 = new List<Bitmap>();
        List<Bitmap> 최신사진첩 = new List<Bitmap>();
        List<파츠출석부> test1 = new List<파츠출석부>();

        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // -- 이제 추가로 등록할 단축키를 아래 List에 추가하면 알아서 Register/UnregisterHotKey를 사용합니다.
        // -- 여기에 단축키를 넣거나 빼고 WndProc만 수정하시면 됩니다.
        private List<Keys> 사용하는단축키들 = new List<Keys> { Keys.NumPad1, Keys.NumPad8, Keys.NumPad9, /*Keys.Space, /*Keys.Q,*/ Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.Add, Keys.Subtract, Keys.NumPad0, Keys.Multiply, Keys.Divide, Keys.PageUp, Keys.PageDown, Keys.End, Keys.Insert, Keys.Delete };

        protected override void OnLoad(EventArgs e) // Form1_Load 안쓰임. 대신 이게 쓰임.
        {
            base.OnLoad(e);



            데이터베이스.무결성검사();
            Test();
            //이미지로드테스트();

            // -- List를 참고하여 알아서 단축키 등록
            foreach (var 단축키코드 in 사용하는단축키들)
            {
                RegisterHotKey(Handle,
                    (int)단축키코드,
                    0x0,
                    (uint)단축키코드
                );
            }
        }
        private void Test()
        {
            var FormModel = new 시세정보Form
            {
                서버 = ServerType.베라,
                아이템이름 = "아쿠아틱",
                시세가 = 5000000,
                옵션정보 = new List<ItemOption> {
                    new ItemOption{ Stat=StatType.힘, Value=18},
                    new ItemOption{ Stat=StatType.Luck, Value=18},
                    new ItemOption{ Stat=StatType.올스탯, Value=5},
                    new ItemOption{ Stat=StatType.업그레이드가능횟수, Value=3},
                }
            };
            var Result = FormModel.조회();
        }
        private Bitmap 색변환(Bitmap bitmap)
        {
            var regen = ApplyGamma(bitmap, 20.0f, 1.8f); // cyan RGB 0/255/255 & Yellow RGB 255/255/0
            MemoryStream ms = new MemoryStream();
            //to4bit(regen, ms);
            ms.Seek(0, SeekOrigin.Begin);
            File.WriteAllBytes("이미지로드테스트.bmp", ms.ToArray());

            return regen;

        }

        void 붙여넣기()
        {
            DD_key(600, 1); Send("v"); DD_key(600, 2);
        }
        private void to4bit(Bitmap sourceBitmap, Stream outputStream)
        {
            BitmapImage myBitmapImage = ToBitmapImage(sourceBitmap);
            FormatConvertedBitmap fcb = new FormatConvertedBitmap();
            fcb.BeginInit();
            myBitmapImage.DecodePixelWidth = sourceBitmap.Width;
            fcb.Source = myBitmapImage;
            fcb.DestinationPalette = Get4BitColorPalette();
            fcb.DestinationFormat = System.Windows.Media.PixelFormats.Indexed4;
            fcb.EndInit();

            PngBitmapEncoder bme = new PngBitmapEncoder();
            bme.Frames.Add(BitmapFrame.Create(fcb));
            bme.Save(outputStream);
        }
        private BitmapPalette Get4BitColorPalette()
        {
            var ColorCodes = new string[] {
                    "#000000",
                    "#0000AA",
                    "#00AA00",
                    "#00AAAA",
                    "#AA0000",
                    "#AA00AA",
                    "#AA5500",
                    "#AAAAAA",
                    "#555555",
                    "#5555FF",
                    "#55FF55",
                    "#55FFFF",
                    "#FF5555",
                    "#FF55FF",
                    "#FFFF55",
                    "#FFFFFF",
            };

            var ColorList = ColorCodes
                            .Select(rgbCode => ColorTranslator.FromHtml(rgbCode))
                            .Select(rgb => System.Windows.Media.Color.FromRgb(rgb.R, rgb.G, rgb.B))
                            .ToList();

            //var ColorList = Enumerable.Range(0x00,0xFF)
            //                .Select(rgbCode => ColorTranslator.FromHtml("#" + rgbCode.ToString("X3")))
            //                .Select(rgb => System.Windows.Media.Color.FromRgb(rgb.R, rgb.G, rgb.B))
            //                .ToList();
            return new BitmapPalette(ColorList);
        }
        private BitmapImage ToBitmapImage(Bitmap sourceBitmap)
        {
            using (var memory = new MemoryStream())
            {
                sourceBitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
        private static Bitmap ApplyGamma(Bitmap bmp0, float gamma, float contrast)
        {

            Bitmap bmp1 = new Bitmap(bmp0.Width, bmp0.Height);
            using (Graphics g = Graphics.FromImage(bmp1))
            {
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                        {
                    new float[] {contrast, 0, 0, 0, 0},
                    new float[] {0,contrast, 0, 0, 0},
                    new float[] {0, 0, contrast, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                        });


                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default,
                                                       ColorAdjustType.Bitmap);
                attributes.SetGamma(gamma, ColorAdjustType.Bitmap);
                g.DrawImage(bmp0, new Rectangle(0, 0, bmp0.Width, bmp0.Height),
                            0, 0, bmp0.Width, bmp0.Height, GraphicsUnit.Pixel, attributes);
            }
            return bmp1;
        }
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        public int 가격OCR(Bitmap bitmap, 좌표 좌표)
        {
            int sum = 0;
            for (int j = 1; j < 5; j++)
            {
                for (int i = 0; i < 10; i++)
                {
                    Bitmap 숫자 = ConvertToFormat(new Bitmap(경로변환("2_" + i.ToString())));
                    if (CompareBitmaps(ConvertToFormat(crop(bitmap, (좌표.x - 2) - (6 * j), 좌표.y, 숫자.Width, 숫자.Height)), ConvertToFormat(숫자)))
                    {
                        sum += i * (int)(Math.Pow(10, (double)j - 1));
                        break;
                    }
                }
            }
            return sum;
        }
        public string[] 페이지가격읽기(string 어느탭)
        {
            Bitmap 원본 = new Bitmap(119, 495);
            Graphics graphics = Graphics.FromImage(원본);
            graphics.CopyFromScreen(588, 208, 0, 0, 원본.Size);
            원본 = ConvertToFormat(색변환(원본)); // 스크린샷 찍고, 색변환
            //Clipboard.SetImage(원본);
            Bitmap[] cropped = new Bitmap[9]; int index = 0;
            string[] 가격 = new string[9];
            foreach (var 자른것 in cropped)
            {
                cropped[index] = new Bitmap(119, 55);
                cropped[index] = ConvertToFormat(crop(원본, 0, (55 * index), 119, 55));
                가격[index] = 가격한줄읽기(cropped[index], 어느탭);
                index++;
            }
            return 가격;
        }
        bool is_axie_died(int 엑시위치)
        {
            winactivate();
            좌표 endturn자리 = new 좌표() { x = 1094, y = 501, 넓이 = 179, 높이 = 75 };
            while (true)
            {
                if (endturn_search("endturn", endturn자리))
                    break;
                대기(딜레이(1000));
            }
            Bitmap screenshot = new Bitmap(10, 10);
            switch (엑시위치)
            {
                case 0:
                    screenshot = 스샷(667, 458, 101, 20);
                    if (imageMatching(screenshot, 이미지("0_pos")).Count > 0 ||
                        imageMatching(screenshot, 이미지("0_pos_1")).Count > 0)
                        return false; break;
                case 1:
                    screenshot = 스샷(757, 379, 101, 20);
                    if (imageMatching(screenshot, 이미지("1_pos")).Count > 0 ||
                        imageMatching(screenshot, 이미지("1_pos_1")).Count > 0)
                        return false; break;
                case 2:
                    screenshot = 스샷(757, 539, 101, 20);
                    if (imageMatching(screenshot, 이미지("2_pos")).Count > 0 ||
                        imageMatching(screenshot, 이미지("2_pos_1")).Count > 0)
                        return false; break;
                case 3:
                    screenshot = 스샷(846, 458, 101, 20);
                    if (imageMatching(screenshot, 이미지("3_pos")).Count > 0 ||
                        imageMatching(screenshot, 이미지("3_pos_1")).Count > 0)
                        return false; break;
                case 4:
                    screenshot = 스샷(936, 379, 101, 20);
                    if (imageMatching(screenshot, 이미지("4_pos")).Count > 0 ||
                        imageMatching(screenshot, 이미지("4_pos_1")).Count > 0)
                        return false; break;
                case 5:
                    screenshot = 스샷(936, 539, 101, 20);
                    if (imageMatching(screenshot, 이미지("5_pos")).Count > 0 ||
                        imageMatching(screenshot, 이미지("5_pos_1")).Count > 0)
                        return false; break;
                case 6:
                    screenshot = 스샷(1026, 458, 101, 20);
                    if (imageMatching(screenshot, 이미지("6_pos")).Count > 0 ||
                        imageMatching(screenshot, 이미지("6_pos_1")).Count > 0)
                        return false; break;
            }
            대기(딜레이(100));
            Bitmap xmark = 스샷(1159, 44, 92, 98);
            
            if (imageMatching(xmark, 이미지("스킬창닫기")).Count > 0)
            {
                mouseclick(1200, 85);
                return false;
            }
            else
                return true;

        }
        
        Bitmap 이미지(string name)
        {
            return ConvertToFormat(new Bitmap(경로변환(name)));
        }
        public void 카드읽기()
        {
            게임초기화();
            int cnt = 0;
            for (int i = 0; i < 7; i++)
            {
                if (cnt == 3) // 엑시 클릭 횟수
                    break;

                switch (i)
                {
                    case 0: mouseclick(710, 437); break;
                    case 1: mouseclick(800, 353); break;
                    case 2: mouseclick(810, 515); break;
                    case 3: 대기(딜레이(700)); mouseclick(892, 432); break;
                    case 4: mouseclick(996, 353); break;
                    case 5: mouseclick(996, 515); break;
                    case 6: mouseclick(1073, 440); break;
                }
                대기(딜레이(100));
                Bitmap xmark = new Bitmap(92, 98);
                Graphics graphics = Graphics.FromImage(xmark);
                graphics.CopyFromScreen(1159, 44, 0, 0, xmark.Size);
                xmark = ConvertToFormat(xmark); // 스크린샷 찍고, 색변환

                if (imageMatching(xmark, 이미지("스킬창닫기")).Count > 0)
                {
                    //Clipboard.SetImage(xmark);
                    //Clipboard.SetImage(ConvertToFormat(new Bitmap(경로변환("스킬창닫기"))));

                    cnt++;
                    
                    Bitmap 카드1 = 스샷(133, 372, 266, 325);
                    Bitmap 카드2 = 스샷(398, 372, 266, 325);
                    Bitmap 카드3 = 스샷(265 + 398, 372, 266, 325);
                    Bitmap 카드4 = 스샷(530 + 398, 372, 266, 325);
                    List<Bitmap> 검열 = new List<Bitmap> { 카드1, 카드2, 카드3, 카드4 };
                    int index = 0 + ((cnt - 1) * 4);
                    foreach (var 다음 in 검열)
                    {
                        //Clipboard.SetImage(다음);
                        Bitmap 검사 = crop(다음, 82, 32, 134, 23); //crop(다음, 40, 24, 25, 35);
                        검사 = ConvertToFormat(검사); // 스크린샷 찍고, 색변환
                        //Clipboard.SetImage(검사);
                        if (imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("cattail slap")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("disguise")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("sunder claw")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("mystic rush")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("tail slap")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("vine dagger")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("cockadoodledoo")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("triple threat")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("luna absorb")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("all-out shot")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("refresh")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("cleanse scent")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("venom spray")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("balloon pop")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("twin needle")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("grub explode")))).Count > 0 ||
                            imageMatching(검사, ConvertToFormat(new Bitmap(경로변환("heroic reward")))).Count > 0)
                        {
                            switch (index)
                            {

                                case 0: invoking(checkBox1, true); invoking(checkBox1, CheckState.Checked); break;
                                case 1: invoking(checkBox2, true); invoking(checkBox2, CheckState.Checked); break;
                                case 2: invoking(checkBox3, true); invoking(checkBox3, CheckState.Checked); break;
                                case 3: invoking(checkBox4, true); invoking(checkBox4, CheckState.Checked); break;
                                case 4: invoking(checkBox5, true); invoking(checkBox5, CheckState.Checked); break;
                                case 5: invoking(checkBox6, true); invoking(checkBox6, CheckState.Checked); break;
                                case 6: invoking(checkBox7, true); invoking(checkBox7, CheckState.Checked); break;
                                case 7: invoking(checkBox8, true); invoking(checkBox8, CheckState.Checked); break;
                                case 8: invoking(checkBox9, true); invoking(checkBox9, CheckState.Checked); break;
                                case 9: invoking(checkBox10, true); invoking(checkBox10, CheckState.Checked); break;
                                case 10: invoking(checkBox11, true); invoking(checkBox11, CheckState.Checked); break;
                                case 11: invoking(checkBox12, true); invoking(checkBox12, CheckState.Checked); break;
                            }
                        }
                        index++;
                    }
                    if (순서 == 0)
                    {
                        pictureBox1.Image = 카드1; 엑시정보.Where(a => a.번호 == 1).Single().사진 = 카드1; 엑시정보.Where(a => a.번호 == 1).Single().위치 = i;
                        pictureBox2.Image = 카드2; 엑시정보.Where(a => a.번호 == 2).Single().사진 = 카드2; 엑시정보.Where(a => a.번호 == 2).Single().위치 = i;
                        pictureBox3.Image = 카드3; 엑시정보.Where(a => a.번호 == 3).Single().사진 = 카드3; 엑시정보.Where(a => a.번호 == 3).Single().위치 = i;
                        pictureBox4.Image = 카드4; 엑시정보.Where(a => a.번호 == 4).Single().사진 = 카드4; 엑시정보.Where(a => a.번호 == 4).Single().위치 = i;
                    }
                    if (순서 == 1)
                    {
                        pictureBox5.Image = 카드1; 엑시정보.Where(a => a.번호 == 5).Single().사진 = 카드1; 엑시정보.Where(a => a.번호 == 5).Single().위치 = i;
                        pictureBox6.Image = 카드2; 엑시정보.Where(a => a.번호 == 6).Single().사진 = 카드2; 엑시정보.Where(a => a.번호 == 6).Single().위치 = i;
                        pictureBox7.Image = 카드3; 엑시정보.Where(a => a.번호 == 7).Single().사진 = 카드3; 엑시정보.Where(a => a.번호 == 7).Single().위치 = i;
                        pictureBox8.Image = 카드4; 엑시정보.Where(a => a.번호 == 8).Single().사진 = 카드4; 엑시정보.Where(a => a.번호 == 8).Single().위치 = i;
                    }
                    if (순서 == 2)
                    {
                        pictureBox9.Image = 카드1; 엑시정보.Where(a => a.번호 == 9).Single().사진 = 카드1; 엑시정보.Where(a => a.번호 == 9).Single().위치 = i;
                        pictureBox10.Image = 카드2; 엑시정보.Where(a => a.번호 == 10).Single().사진 = 카드2; 엑시정보.Where(a => a.번호 == 10).Single().위치 = i;
                        pictureBox11.Image = 카드3; 엑시정보.Where(a => a.번호 == 11).Single().사진 = 카드3; 엑시정보.Where(a => a.번호 == 11).Single().위치 = i;
                        pictureBox12.Image = 카드4; 엑시정보.Where(a => a.번호 == 12).Single().사진 = 카드4; 엑시정보.Where(a => a.번호 == 12).Single().위치 = i;
                    }
                    순서++;
                    if (순서 == 3)
                        순서 = 0;
                    mouseclick(1200, 85);
                    대기(딜레이(150));
                    newgame_checked = true;
                }
            }
            return;
        }

        private static System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {
            //Get the image current width  
            int sourceWidth = imgToResize.Width;
            //Get the image current height  
            int sourceHeight = imgToResize.Height;
            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            //Calulate  width with new desired size  
            nPercentW = ((float)size.Width / (float)sourceWidth);
            //Calculate height with new desired size  
            nPercentH = ((float)size.Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            //New Width  
            int destWidth = (int)(sourceWidth * nPercent);
            //New Height  
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            // Draw image with new width and height  
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            return (System.Drawing.Image)b;
        }

        public string 가격한줄읽기(Bitmap bitmap, string 어느탭)
        {
            string filename = ""; string 만 = ""; string 억 = "";
            string result = ""; int sum = 0;
            switch (어느탭)
            {
                case "검색탭":
                    filename = "2_"; 만 = "검색탭_만"; 억 = "검색탭_억";
                    break;
                case "시세탭":
                    filename = "4_"; 만 = "시세탭_만"; 억 = "시세탭_억";
                    break;
            }

            List<좌표> 만자리 = imageMatching(bitmap, ConvertToFormat(new Bitmap(경로변환(만))));
            List<좌표> 억자리 = imageMatching(bitmap, ConvertToFormat(new Bitmap(경로변환(억))));
            Dictionary<좌표, int> digits = new Dictionary<좌표, int> { };
            if (만자리.Count > 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    Bitmap 숫자 = ConvertToFormat(new Bitmap(경로변환(filename + i.ToString())));
                    var 임시 = imageMatching(ConvertToFormat(crop(bitmap, 0, 만자리.Single().y, 만자리.Single().x + 40, 만자리.Single().높이)), 숫자);
                    foreach (var coord in 임시)
                    {
                        digits.Add(coord, i);
                    }
                }
            }
            else
                return "";

            var queryAsc = digits.OrderBy(x => x.Key.x);
            foreach (var a in queryAsc)
            {
                result += a.Value.ToString();
            }
            return result;
        }
        public string 숫자OCR(string filename, 좌표 시작점) => 숫자OCR(filename, 시작점.x, 시작점.y, 시작점.넓이, 시작점.높이);
        public string 숫자OCR(string filename, int x, int y, int w, int h)
        {
            string result = "";
            Dictionary<좌표, int> digits = new Dictionary<좌표, int> { };
            for (int i = 0; i < 10; i++)
            {
                var 임시 = imageCoords(filename + i, x, y, w, h);
                foreach (var coord in 임시)
                {
                    digits.Add(coord, i);
                }
            }

            var queryAsc = digits.OrderBy(a => a.Key.x);
            foreach (var a in queryAsc)
            {
                result += a.Value.ToString();
            }
            return result;
        }

        public void 시세탭_가격분석()
        {
            winactivate();
            Task.Run(async () =>
            {
                await sise_setting();

            }).Wait();

            mouseclick(rand(좌표목록["시세"] as 좌표) as int[]); 대기(딜레이(500));
            mouseclick(rand(좌표목록["검색시작"] as 좌표) as int[]); 대기(딜레이(500));
            send_enter(5); 대기(딜레이(500));
            List<시세정보Form> Temp = new List<시세정보Form> { };
            string 아이템이름 = "초보자활성";
            int samedata_counter = 0;
            string 전체페이지, 현재페이지;
            //페이지 넘기기
            좌표 슬래시 = imageCoords("슬래시", 608, 154, 63, 16).Single();
            전체페이지 = 숫자OCR("3_", 슬래시.x + 슬래시.넓이, 슬래시.y, 30, 16);
            string 날짜 = 숫자OCR("4_", 885, 224, 76, 19);
            do
            {
                슬래시 = imageCoords("슬래시", 608, 154, 63, 16).Single();
                현재페이지 = 숫자OCR("3_", 슬래시.x - 30, 슬래시.y, 30, 16);
                string[] 가격 = 페이지가격읽기("시세탭");

                //if (현재페이지 == "1")
                //{
                //    for (int i = 0; i < 5; i++)
                //    {
                //        prices.Add(new first5prices() { index = i, price = 가격[i], serverType = 현재서버 });
                //    }
                //}
                for (int 줄 = 0; 줄 < 9; 줄++)
                {
                    if (가격[줄] == "")
                        break;
                    if (날짜 != 숫자OCR("4_", 885, (55 * 줄) + 224, 76, 19))
                    {
                        현재페이지 = 전체페이지;
                        break;
                    }
                    List<ItemOption> 옵션 = 옵션읽기(줄, 아이템이름);
                    if (옵션 == null)
                    {
                        실행확인();
                        Task.Run(async () =>
                        {
                            await 로그인(975);
                        }).Wait();
                        서버입장(현재서버);
                        캐릭입장(현재서버);
                        경매장입장();
                        시세탭_가격분석();
                        return;

                    }
                    var 시세정보 = new FormModels.시세정보Form
                    {
                        서버 = 현재서버,
                        시세가 = decimal.Parse(가격[줄]),
                        아이템이름 = "아쿠아틱",
                        옵션정보 = 옵션
                    };
                    var result = 시세정보.조회();
                    var ParsedStat = 옵션.ToList();


                    /************************** 가격만 순서대로 연속적으로 저장된 숫자가 발견되면, 종료하는 방법으로 변경 추진  ****************************/


                    //if (가격[줄] == prices.Where(a => a.index == samedata_counter && 현재서버 == a.serverType).Single().price)
                    //{
                    //    Temp.Add(시세정보);
                    //    samedata_counter++;
                    //}
                    //else
                    //{
                    //    if (Temp.Count > 0)
                    //    {
                    //        foreach (var t in Temp)
                    //            t.저장().GetAwaiter().GetResult();
                    //        Temp.Clear();
                    //    }
                    //    시세정보.저장().GetAwaiter().GetResult();
                    //    samedata_counter = 0;
                    //    break;
                    //}
                    //if (samedata_counter == 4)                                                                                                     
                    //{
                    //    현재페이지 = 전체페이지;
                    //    break;
                    //}


                    /************************** 가격만 순서대로 연속적으로 저장된 숫자가 발견되면, 종료하는 방법으로 변경 추진  ****************************/


                    //PriceInfo 가격만 = new PriceInfo();
                    if (result.Count() > 0)
                    {
                        foreach (var par in ParsedStat)
                        {
                            //var 임시 = result.Where(a => a.PriceInfo2ItemOptions.Where(b => b.ItemOption.Stat == par).Select(b=>b.ItemOption.Value) == 옵션.Where(b=>b.Stat ==par ).Select(b=>b.Value)).Single();
                            par.Id = (from a in 데이터베이스.접근.ItemOptions
                                      where a.Stat == par.Stat && a.Value == par.Value
                                      select a.Id).SingleOrDefault();
                            if (par.Id < 1)
                            {
                                if (Temp.Count > 0)
                                {
                                    foreach (var t in Temp)
                                        t.저장().GetAwaiter().GetResult();
                                    Temp.Clear();
                                }
                                시세정보.저장().GetAwaiter().GetResult();
                                samedata_counter = 0;
                                break;
                            }

                        }

                        if (ParsedStat.All(a => a.Id > 0))
                        {
                            int result_counter = result.Count();
                            foreach (var re in result)
                            {
                                int parsed_counter = ParsedStat.Count();
                                foreach (var par in ParsedStat)
                                {
                                    if (re.PriceInfo2ItemOptions.Any(a => a.ItemOptionId == par.Id && a.PriceInfo.Server == 현재서버))
                                    {
                                        parsed_counter--;
                                    }
                                    else
                                        break;
                                    if (parsed_counter < 1)
                                    {
                                        Temp.Add(시세정보);
                                        samedata_counter++;
                                    }
                                }
                                if (parsed_counter == 0)
                                    break;
                                result_counter--;
                                if (result_counter == 0 && parsed_counter > 0)
                                {
                                    if (Temp.Count > 0)
                                    {
                                        foreach (var t in Temp)
                                            t.저장().GetAwaiter().GetResult();
                                        Temp.Clear();
                                    }
                                    시세정보.저장().GetAwaiter().GetResult();
                                    samedata_counter = 0;
                                }
                            }
                        }

                    }
                    else
                    {
                        if (Temp.Count > 0)
                        {
                            foreach (var t in Temp)
                                t.저장().GetAwaiter().GetResult();
                            Temp.Clear();
                        }
                        시세정보.저장().GetAwaiter().GetResult();
                        samedata_counter = 0;
                    }

                    if (samedata_counter == 3)                                                                                                     // 근데 들어온 임시데이터가 3개가  연속적으로 있다면, 다 검색한거니까 끝.
                    {
                        현재페이지 = 전체페이지;
                        break;
                    }

                }
                mousemove(rand(672, 346, 5, 5)); mousemove(rand(672, 346, 5, 5));
                mre.WaitOne();
                mouseclick(rand(673, 161, 5, 5)); 대기(딜레이(100));

            } while (현재페이지 != 전체페이지);
            return;
        }
        public string[] 검색탭_가격(int 몇줄읽을까, int 올스탯) => 검색탭_가격(몇줄읽을까, "", 0, 올스탯);
        public string[] 검색탭_가격(int 몇줄읽을까, string 추가옵션, int 수치, int 올스탯)
        {
            string[] 완료 = new string[10];
            int 줄 = 0; 좌표 임시좌표;
            int[] 백만자리입니까 = new int[10];
            int[,] 결과 = new int[10, 10];
            switch (추가옵션)
            {
                case "힘": 추가옵션 = "0"; 수치 -= 6; break;
                case "덱": 추가옵션 = "1"; 수치 -= 6; break;
                case "인": 추가옵션 = "2"; 수치 -= 6; break;
                case "럭": 추가옵션 = "3"; 수치 -= 6; break;
                case "공": 추가옵션 = "4"; 수치 -= 1; break;
                case "마": 추가옵션 = "5"; 수치 -= 1; break;
            }

            string[] 가격 = 페이지가격읽기("검색탭");

            if (수치 == 0)
            {
                for (int j = 0; j < 몇줄읽을까; j++)
                    완료[j] = 가격[j];
                return 완료;
            }

            ItemOption 올스텟옵션 = null, 업횟옵션 = null, 추가옵션수치 = null;
            for (int j = 0; j < 몇줄읽을까; j++)
            {
                var 옵션 = 옵션읽기(줄, "초보자활성");
                올스텟옵션 = 옵션.SingleOrDefault(op => op.Stat == StatType.올스탯) ?? new ItemOption();
                업횟옵션 = 옵션.SingleOrDefault(op => op.Stat == StatType.업그레이드가능횟수) ?? new ItemOption();
                추가옵션수치 = 옵션.SingleOrDefault(op => op.Stat == (StatType)int.Parse(추가옵션)) ?? new ItemOption();

                mousemove(335, 232 + (55 * 줄));
                if (올스텟옵션.Value == 올스탯 && 수치 <= 추가옵션수치.Value && (업횟옵션.Value == 3 || 업횟옵션.Value == 4))
                {
                    완료[j] = 가격[줄];
                    줄++;
                }
                else
                {
                    줄++;
                    j--;
                }
                if (줄 == 9)
                {
                    줄 = 0;
                    좌표 임시 = new 좌표() { x = 671, y = 158, 넓이 = 16, 높이 = 9 };
                    mouseclick(rand(임시));
                    대기(딜레이(200));
                    가격 = 페이지가격읽기("검색탭");
                }

            }
            return 완료;
        }

        public static string 완료탭_가격()
        {
            string imgPath;
            int swap = 0;
            int 검색숫자 = 0; //0~9까지
            int 백만자리입니까 = 0;
            int[] 결과 = new int[10];
            int 만위치y = 363; int 만위치x = 515;
            int 백만시작좌표 = 493; int 천만시작좌표 = 490;
            imgPath = "img\\만.png";
            if (UseImageSearch(imgPath, 만위치x, 만위치y, 만위치x + 11, 만위치y + 11) != null)
            {
                백만자리입니까 = 0;
                for (int i = 0; i < 3; i++) //첫째자리부터 3째자리까지
                {
                    for (검색숫자 = 9; 검색숫자 >= 0; 검색숫자--) // 0, 9 8 1 5 6 7 4 3 2
                    {
                        swap = 검색숫자 * 111;
                        string 경로 = "img\\" + swap + ".png";
                        if (UseImageSearch(경로, 백만시작좌표 + (i * 7), 만위치y, 백만시작좌표 + (i * 7) + 7, 만위치y + 11) != null)
                        {
                            결과[i] = 검색숫자;
                            break;
                        }
                    }
                }
            }
            else if (UseImageSearch(imgPath, 만위치x + 4, 만위치y, 만위치x + 4 + 11, 만위치y + 11) != null)// 천만자리이면
            {
                백만자리입니까 = 1;
                for (int i = 0; i < 4; i++) //첫번째자리부터 4째자리까지
                {
                    for (검색숫자 = 9; 검색숫자 >= 0; 검색숫자--)
                    {
                        swap = 검색숫자 * 111;
                        string 경로 = "img\\" + swap + ".png";
                        if (UseImageSearch(경로, 천만시작좌표 + (i * 7), 만위치y, 천만시작좌표 + (i * 7) + 7, 만위치y + 11) != null)
                        {
                            결과[i] = 검색숫자;
                            break;
                        }
                    }
                }
            }
            else
            {
                백만자리입니까 = 2;
                for (검색숫자 = 9; 검색숫자 >= 0; 검색숫자--)
                {
                    swap = 검색숫자 * 111;
                    string 경로 = "img\\" + swap + ".png";
                    if (UseImageSearch(경로, 백만시작좌표 - 15, 만위치y, 백만시작좌표 - 15 + 7, 만위치y + 11) != null)
                    {
                        결과[0] = 검색숫자;
                        break;
                    }
                }

                for (int i = 0; i < 4; i++) //첫번째자리부터 4째자리까지
                {
                    for (검색숫자 = 9; 검색숫자 >= 0; 검색숫자--)
                    {
                        swap = 검색숫자 * 111;
                        string 경로 = "img\\" + swap + ".png";
                        if (UseImageSearch(경로, 백만시작좌표 + 8 + (i * 7), 만위치y, 백만시작좌표 + 8 + (i * 7) + 7, 만위치y + 11) != null)
                        {
                            결과[i + 1] = 검색숫자;
                            break;
                        }
                    }
                }
            }
            string 임시완료 = string.Join("", 결과);
            if (백만자리입니까 == 0)
                임시완료 = 임시완료.Substring(0, 7);
            else if (백만자리입니까 == 1)
                임시완료 = 임시완료.Substring(0, 8);
            else
                임시완료 = 임시완료.Substring(0, 9);

            return 임시완료;
        }

        public static 좌표 UseImageSearch(string imgPath, string tolerance) => UseImageSearch(imgPath, 0, 0, 1920, 1080, tolerance);
        public static 좌표 UseImageSearch(string imgPath, int x1, int y1, int x2, int y2, string tolerance = "30")
        {
            Image image1 = Image.FromFile(imgPath);

            int w = image1.Width;
            int h = image1.Height;
            imgPath = $"*{tolerance} {imgPath}"; // "*" + tolerance + " " + imgPath;

            IntPtr result = ImageSearch(x1, y1, x2, y2, imgPath);
            string res = Marshal.PtrToStringAnsi(result);

            int temp = default(int);
            var NotParsed = res.Split('|').Any(s => !int.TryParse(s, out temp)); //쪼개진 결과값 중 하나라도 int로 변환하는데 실패하면 true

            if (res[0] == '0' || NotParsed == true) return null;

            var data = res.Split('|').Select(s => int.Parse(s)).ToArray();

            return new 좌표
            {
                x = data[1],
                y = data[2],
                넓이 = data[3],
                높이 = data[4]
            };
        }

        private void 실행확인()
        {
            Process[] processList = Process.GetProcessesByName("MapleStory");
            if (processList.Length < 1)
            {
                메이플실행();
            }
            else
            {

            }
        }
        public static Bitmap ConvertToFormat(Image image)
        {
            Bitmap copy = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
            using (Graphics gr = Graphics.FromImage(copy))
            {
                gr.DrawImage(image, new Rectangle(0, 0, copy.Width, copy.Height));
            }
            return copy;
        }
        public string 경로변환(string str)
        {
            str = "img\\" + str + ".png";
            return str;
        }
        public Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }

            return result;
        }
        public Bitmap crop(Bitmap img, int x, int y, int width, int height)
        {
            Rectangle cropRect = new Rectangle(x, y, width, height);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(img, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }
            return target;
        }
        public static bool CompareBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1 == null || bmp2 == null)
                return false;
            if (object.Equals(bmp1, bmp2))
                return true;
            if (!bmp1.Size.Equals(bmp2.Size) || !bmp1.PixelFormat.Equals(bmp2.PixelFormat))
                return false;

            int bytes = bmp1.Width * bmp1.Height * (Image.GetPixelFormatSize(bmp1.PixelFormat) / 8);

            bool result = true;
            byte[] b1bytes = new byte[bytes];
            byte[] b2bytes = new byte[bytes];

            BitmapData bitmapData1 = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width, bmp1.Height), ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData bitmapData2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), ImageLockMode.ReadOnly, bmp2.PixelFormat);

            Marshal.Copy(bitmapData1.Scan0, b1bytes, 0, bytes);
            Marshal.Copy(bitmapData2.Scan0, b2bytes, 0, bytes);

            for (int n = 0; n <= bytes - 1; n++)
            {
                if (b1bytes[n] != b2bytes[n])
                {
                    result = false;
                    break;
                }
            }

            bmp1.UnlockBits(bitmapData1);
            bmp2.UnlockBits(bitmapData2);

            return result;
        }
        public Bitmap 아쿠아틱옵션읽기(int sourceX, int sourceY)
        {
            Bitmap bitmap = new Bitmap(143, 347);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(sourceX, sourceY, 0, 0, bitmap.Size);
            bitmap = ConvertToFormat(색변환(bitmap)); // 스크린샷 찍고, 색변환
            return bitmap;
        }
        public List<좌표> imageCoords(string filename, int x, int y, int w, int h)
        {
            Bitmap subimage = ConvertToFormat(new Bitmap(경로변환(filename)));
            Bitmap source = new Bitmap(w, h);
            Graphics graphics = Graphics.FromImage(source);
            graphics.CopyFromScreen(x, y, 0, 0, source.Size);
            source = ConvertToFormat(색변환(source)); // 스크린샷 찍고, 색변환
            //Clipboard.SetImage(source);

            var Matched = imageMatching(source, subimage);
            List<좌표> result = new List<좌표>();
            foreach (var match in Matched)
            {
                match.x += x;
                match.y += y;
                result.Add(match);
            }

            return result;

        }
        public List<좌표> imageMatching(Bitmap source, Bitmap subimage, int x = 0, int y = 0, int w = 0, int h = 0)
        {
            List<좌표> 찾은결과 = new List<좌표>();
            ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.95f);
            TemplateMatch[] matchings = tm.ProcessImage(source, ConvertToFormat(subimage)); // 찍은 스크린샷에서 옵션을 검색합니다.
            foreach (var match in matchings)
            {
                Bitmap croppedsource = ConvertToFormat(crop(source, match.Rectangle.X, match.Rectangle.Y, subimage.Width, subimage.Height));
                if (CompareBitmaps(ConvertToFormat(croppedsource), subimage))
                {
                    찾은결과.Add(new 좌표 { x = match.Rectangle.X, y = match.Rectangle.Y, 넓이 = subimage.Width, 높이 = subimage.Height });
                    //좌표 result = new 좌표 {x=match.Rectangle.X, y=match.Rectangle.Y, 넓이=subimage.Width, 높이=subimage.Height};
                }
                //찾은결과.SingleOrDefault(i => i.높이==200);
            }

            return 찾은결과;

        }
        public List<ItemOption> 옵션읽기(int line, string itemname)
        {

            List<ItemOption> 값 = new List<ItemOption>();
            string[] 옵션 = new string[] { "aSTR", "aDEX", "aINT", "aLUK", "a공격력", "a마력", "a올스탯", "업횟" };
            string[] 잠재등급 = new string[] { "에픽", "유니크", "레전드리" };
            string[] 잠재옵션 = new string[] { "STR_jamje", "DEX_jamje", "INT_jamje", "LUK_jamje", "올스탯_jamje" };
            int i = 0, 인덱스 = 0;
            좌표 itemCoord; List<좌표> 매칭; Dictionary<좌표, String> 발견잠재 = new Dictionary<좌표, string> { };
            Bitmap cropped;
            DateTime 시작시간 = DateTime.Now;// 현재시간 찍기.
            do     //아쿠아틱 옵션 팝업 안보이면 값 받을때까지 반복
            {
                mousemove(334, 232 + (55 * line));
                mousemove(291, 232 + (55 * line));
                대기(딜레이(50));
                //itemCoord = 화면전환대기(itemname, 348, 302, 348+65, 302+100);
                itemCoord = (UseImageSearch("img\\" + itemname + ".png", 348, 302, 348 + 65, 302 + 100, "30"));

                TimeSpan 경과시간 = DateTime.Now - 시작시간;
                if (경과시간.TotalSeconds > 60)
                {
                    return null;
                }

            } while (itemCoord == null);

            Bitmap 원본 = new Bitmap(143, 347);
            Graphics graphics = Graphics.FromImage(원본);
            graphics.CopyFromScreen(itemCoord.x - 29 + 12 + 6, itemCoord.y + 126 + 22 - 96, 0, 0, 원본.Size);
            원본 = ConvertToFormat(색변환(원본)); // 스크린샷 찍고, 색변환
            //Clipboard.SetImage(원본);
            foreach (var 옵 in 옵션)                                           //옵션별로 수치를 검색합니다.
            {
                매칭 = imageMatching(원본, ConvertToFormat(new Bitmap(경로변환(옵))));
                if (매칭 != null && 매칭.Count() > 0)                                     // 만약에 추가옵션이 붙었으면, 
                {
                    int cnt = 0; int[] 순서 = new int[2]; int[] 수치 = new int[2];
                    cropped = ConvertToFormat(crop(원본, 매칭.Single().x, 매칭.Single().y, 143, 10)); // 추가옵션붙은 부분 한줄만 잘라서
                    if (옵 == "a올스탯")
                    {
                        for (i = 3; i <= 7; i++)
                        {
                            Bitmap 스탯 = ConvertToFormat(new Bitmap(경로변환("a" + i.ToString())));
                            if (imageMatching(cropped, 스탯).Count > 0)
                                값.Add(new ItemOption { Value = i, Stat = StatType.올스탯 });
                        }

                    }
                    else if (옵 == "업횟")                                               // 만약에 찾은 옵션이 업횟이면,
                    {
                        for (i = 0; i <= 4; i++)
                        {
                            Bitmap 스탯 = ConvertToFormat(new Bitmap(경로변환(i.ToString() + i.ToString())));
                            if (imageMatching(cropped, 스탯).Count > 0)
                                값.Add(new ItemOption { Value = i, Stat = StatType.업그레이드가능횟수 });
                        }
                    }
                    else
                    {
                        for (i = 0; i < 10; i++)
                        {
                            Bitmap 스탯 = ConvertToFormat(new Bitmap(경로변환("a" + i.ToString())));
                            매칭 = imageMatching(cropped, 스탯);
                            if (imageMatching(cropped, 스탯).Count > 0)
                            {
                                foreach (var match in 매칭)
                                {
                                    순서[cnt] = match.x;
                                    수치[cnt] = i;
                                    cnt++;
                                }
                                if (cnt == 2)
                                    break;
                            }
                        }
                        if (cnt != 1)                               // 만약 추가옵션 수치가 2줄이 넘으면
                        {
                            if (순서[0] > 순서[1])                  // 앞뒤를 바꾸고 수치를 저장합니다.
                                swap(ref 수치[0], ref 수치[1]);
                            값.Add(new ItemOption { Value = int.Parse(수치[0].ToString() + 수치[1].ToString()), Stat = (StatType)인덱스 });  // TODO: 반드시 수정 필요 - 수치를 잘못 가져올 가능성이 높음
                        }
                        else
                            //값[인덱스] = 수치[0];                     // 수치가 한자리수면 그냥 저장합니다.
                            값.Add(new ItemOption { Value = 수치[0], Stat = (StatType)인덱스 });  // TODO: 반드시 수정 필요 - 수치를 잘못 가져올 가능성이 높음
                    }
                }
                인덱스++;
            }
            인덱스 = 1;
            foreach (var 등급 in 잠재등급)
            {
                매칭 = imageMatching(원본, ConvertToFormat(new Bitmap(경로변환(등급)))); //  에픽인지 유니크인지 레전인지

                if (매칭 != null && 매칭.Count() > 0) //만약 에픽이거나 유니크거나 레전드리면
                {
                    값.Add(new ItemOption { Value = 인덱스, Stat = StatType.잠재 });
                    cropped = ConvertToFormat(crop(원본, 매칭.First().x, 매칭.First().y, 143, 원본.Height - 매칭.First().y));
                    //Clipboard.SetImage(cropped);
                    foreach (var 잠재 in 잠재옵션)                                          //  잠재 힘덱인럭 검사하기
                    {
                        매칭 = imageMatching(cropped, ConvertToFormat(new Bitmap(경로변환(잠재)))); //잠재가 
                        foreach (var coords in 매칭)
                            발견잠재.Add(coords, 잠재);
                    }
                    foreach (var 잠재 in 발견잠재)
                    {
                        Bitmap 한줄 = ConvertToFormat(crop(cropped, 잠재.Key.x, 잠재.Key.y, 143, 잠재.Key.높이));
                        //Clipboard.SetImage(한줄);
                        for (i = 1; i < 5; i++)
                        {
                            매칭 = imageMatching(한줄, ConvertToFormat(new Bitmap(경로변환((i * 3).ToString() + "%"))));
                            if (매칭.Count() > 0)
                            {
                                ItemOption 발견된잠재옵션 = null;
                                switch (잠재.Value)
                                {
                                    case "STR_jamje": 발견된잠재옵션 = 값.SingleOrDefault(entry => entry.Stat == StatType.힘퍼) ?? new ItemOption { Stat = StatType.힘퍼 }; break;
                                    case "DEX_jamje": 발견된잠재옵션 = 값.SingleOrDefault(entry => entry.Stat == StatType.덱퍼) ?? new ItemOption { Stat = StatType.덱퍼 }; break;
                                    case "INT_jamje": 발견된잠재옵션 = 값.SingleOrDefault(entry => entry.Stat == StatType.인퍼) ?? new ItemOption { Stat = StatType.인퍼 }; break;
                                    case "LUK_jamje": 발견된잠재옵션 = 값.SingleOrDefault(entry => entry.Stat == StatType.럭퍼) ?? new ItemOption { Stat = StatType.럭퍼 }; break;
                                    case "올스탯_jamje": 발견된잠재옵션 = 값.SingleOrDefault(entry => entry.Stat == StatType.올퍼) ?? new ItemOption { Stat = StatType.올퍼 }; break;
                                }

                                발견된잠재옵션.Value += (i * 3);
                                if (!값.Any(a => a.Stat == 발견된잠재옵션.Stat))
                                    값.Add(발견된잠재옵션);
                                break;
                            }

                        }

                    }
                    break;
                }
                인덱스++;
            }

            return 값;
        }
        public void swap(ref int a, ref int b)
        {
            int temp = b;
            b = a;
            a = temp;
        }
        public int[] 판매탭_옵션확인()
        {
            int 옵션시작점x; int i = 0;
            string[] 옵션 = new string[] { "aSTR", "aDEX", "aINT", "aLUK", "a공격력", "a마력", "a올스탯", "업횟" };
            string[] 옵션경로 = new string[8];
            int x1, w2, j, 옵션인덱스 = 0, 옵션간격 = 0, 인덱스 = 0;
            int 값1 = 0, 값2 = 0; bool 옵션값이9 = false;
            int[] 값 = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach (var 옵 in 옵션)
            {
                옵션경로[i] = 경로변환(옵);
                i++;
            }
            ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.96f);
            좌표 y; Image image2;
            do     //아쿠아틱 옵션 팝업 안보이면 값 받을때까지 반복
            {
                y = UseImageSearch(경로변환("아쿠아틱"), "30");
                mouseclick(rand(좌표목록["판매"] as 좌표) as int[]);  // 판매탭에 마우스 올려놨다가
                대기(딜레이(200));
                좌표 임시좌표 = UseImageSearch(경로변환("아쿠아이콘"), 73, 152, 661, 372);  //다시 남은 아쿠아틱 위에 마우스 갖다놓기
                if (임시좌표 != null)
                {
                    mousemove(rand(임시좌표));
                }
                대기(딜레이(200));
            } while (y == null);

            Bitmap 원본 = new Bitmap(143, 347);
            Graphics graphics = Graphics.FromImage(원본);
            graphics.CopyFromScreen(y.x - 29 + 12, y.y + 126 + 22, 0, 0, 원본.Size);
            원본 = ConvertToFormat(색변환(원본)); // 스크린샷 찍고, 색변환
            foreach (var 옵 in 옵션)                                           //옵션별로 수치를 검색합니다.
            {
                List<좌표> 매칭 = imageMatching(원본, ConvertToFormat(new Bitmap(경로변환(옵))));
                if (매칭 != null && 매칭.Count > 0)                                     // 만약에 추가옵션이 붙었으면, 
                {
                    int cnt = 0; int[] 순서 = new int[2]; int[] 수치 = new int[2];
                    Bitmap cropped = ConvertToFormat(crop(원본, 매칭.Single().x, 매칭.Single().y, 143, 10)); // 추가옵션붙은 부분 한줄만 잘라서
                    if (옵 == "올스탯")
                    {
                        for (i = 5; i <= 7; i++)
                        {
                            Bitmap 스탯 = ConvertToFormat(new Bitmap(경로변환("a" + i.ToString())));
                            if (imageMatching(cropped, 스탯).Count > 0)
                                값[인덱스] = i;
                        }
                    }
                    else if (옵 == "업횟")                                               // 만약에 찾은 옵션이 업횟이면,
                    {
                        for (i = 0; i <= 4; i++)
                        {
                            Bitmap 스탯 = ConvertToFormat(new Bitmap(경로변환(i.ToString() + i.ToString())));
                            if (imageMatching(cropped, 스탯).Count > 0)
                                값[인덱스] = i;
                        }
                    }
                    else
                    {
                        for (i = 0; i < 10; i++)
                        {
                            Bitmap 스탯 = ConvertToFormat(new Bitmap(경로변환("a" + i.ToString()))); // 검색할 숫자 즉, 수치이미지 값을 선택하고
                            매칭 = imageMatching(cropped, 스탯);                // 자른 한줄에서 옵션의 수치를 검색합니다
                            if (imageMatching(cropped, 스탯).Count > 0)
                            {
                                foreach (var match in 매칭)
                                {
                                    순서[cnt] = match.x;
                                    수치[cnt] = i;
                                    cnt++;
                                }
                                if (cnt == 2)
                                    break;
                            }
                        }
                        if (cnt != 1)                               // 만약 추가옵션 수치가 2줄이 넘으면
                        {
                            if (순서[0] > 순서[1])                  // 앞뒤를 바꾸고 수치를 저장합니다.
                                swap(ref 수치[0], ref 수치[1]);
                            값[인덱스] = int.Parse(수치[0].ToString() + 수치[1].ToString());
                        }
                        else
                            값[인덱스] = 수치[0];                     // 수치가 한자리수면 그냥 저장합니다.
                    }
                }
                인덱스++;
            }

            return 값;

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // -- 위의 List를 참고하여 알아서 단축키 해제
            foreach (var 단축키코드 in 사용하는단축키들)
            {
                UnregisterHotKey(Handle, 사용하는단축키들.IndexOf(단축키코드));
            }
            /*            Properties.Settings.Default.스카 = textBox1.Text;
                        Properties.Settings.Default.베라 = textBox2.Text;
                        Properties.Settings.Default.루나 = textBox3.Text;
                        Properties.Settings.Default.크로아 = textBox4.Text;
                        Properties.Settings.Default.엘리시움 = textBox5.Text;
                        Properties.Settings.Default.레드 = textBox6.Text;
                        Properties.Settings.Default.오로라 = textBox7.Text;
                        Properties.Settings.Default.딜레이 = textBox8.Text;*/
            Properties.Settings.Default.Save();

            base.OnFormClosing(e);
        }

        public static int[] rand(int x, int y, int w, int h)
        {
            Random r = new Random();

            int[] 결과값 = new int[]
            {
                r.Next(0, w) + x,
                r.Next(0, h) + y
            };
            return 결과값;
        }
        public static int[] rand(좌표 point)
        {
            Random r = new Random();
            int[] 결과값 = new int[] {
                r.Next(0, point.넓이) + point.x,
                r.Next(0, point.높이) + point.y
            };
            return 결과값;
        }

        void winactivate()
        {
            IntPtr hw1 = FindWindow("UnityWndClass", null);
            //MoveWindow(hw1, -7, 0, 1280, 720, false);
            SetWindowPos(hw1, IntPtr.Zero, -7, 0, 0, 0, SWP_NOSIZE | SWP_SHOWWINDOW);


            //int hWnd = hw1.ToInt32();
            if (!hw1.Equals(IntPtr.Zero))
            {
                ShowWindowAsync(hw1, SW_SHOWNORMAL);
                SetForegroundWindow(hw1);
            }
            대기(딜레이(200));
        }

        private void GetSearchedImage(string[] 이미지경로, string 이름, Action<좌표> 보정 = null)
        {
            //이미지 찾고 좌표 받기
            var SearchedImage = 이미지경로.Select(s => UseImageSearch(s, "40"));
            좌표 검색결과 = SearchedImage.FirstOrDefault(i => i != null);

            if (보정 != null)
                보정(검색결과);
            좌표목록[이름] = 검색결과;

            Random r = new Random();
            var Searched = 좌표목록[이름];
            좌표 임시좌표 = new 좌표
            {
                x = Searched.x + r.Next(0, 3),
                y = Searched.y + r.Next(0, 3)
            };

            //DD_mov(임시좌표);
            //대기(딜레이(100));
            var 검색대상 = new string[] { "검색", "검색결과", "완료", "판매" };
            if (검색대상.Contains(이름))
            {
                mouseclick(임시좌표);
            }

        }

        private void 좌표입력(string 이름, int a, int b, int w, int h)
        {
            좌표목록[이름] = new 좌표()
            {
                x = a,
                y = b,
                넓이 = w,
                높이 = h
            };

        }
        private void mouseclick(int x, int y)
        {
            DD_mov(x, y);
            DD_btn(1);
            대기(딜레이(키입력지연));
            DD_btn(2);
            대기(딜레이(키입력지연));
        }
        private void mousemove(int x, int y)
        {
            DD_mov(x, y);
            대기(딜레이(키입력지연));
            대기(딜레이(키입력지연));
        }
        private void mousedrag(int x, int y, int dx, int dy)
        {
            DD_mov(x, y);
            DD_btn(1);
            대기(딜레이(키입력지연));
            DD_mov(dx, dy);
            DD_btn(2);
            대기(딜레이(키입력지연));
        }

        private void send_key(int i, int 횟수 = 1)
        {
            foreach (var count in Enumerable.Range(1, 횟수))
            {
                DD_key(i, 1);
                대기(딜레이(키입력지연));
                DD_key(i, 2);
                대기(딜레이(키입력지연));
            }
        }
        private void 한영키변경()
        {
            한영키변경완료 = true;
            send_key(604);
        }


        private async Task setting()
        {
            if (is_setting_done)
                return;
            winactivate();
            await Task.Delay(딜레이(키입력지연));

            int[] 랜덤결과값;
            좌표목록 = new Dictionary<string, 좌표>() {
                {"검색", new 좌표()},                 {"방어구", new 좌표()},
                {"검색시작", new 좌표()},             {"검색결과", new 좌표()},
                {"구매하기", new 좌표()},             {"완료", new 좌표()},
                {"전체", new 좌표()},                 {"판매완료", new 좌표()},
                {"거래실패", new 좌표()},             {"재등록", new 좌표()},
                {"모두받기", new 좌표()},             {"첫째옵션", new 좌표()},
                {"둘째옵션", new 좌표()},             {"셋째옵션", new 좌표()},
                {"가격", new 좌표()},                 {"빈슬롯", new 좌표()},
                {"판매", new 좌표()},                 {"판매대", new 좌표()},
                {"판매가격", new 좌표()},             {"판매등록", new 좌표()},
                {"시세", new 좌표() }
            };

            GetSearchedImage(new string[] {
                "img\\시세.png",
                "img\\시세1.png"
            }, "시세");
            await Task.Delay(딜레이(키입력지연));

            GetSearchedImage(new string[] {
                "img\\검색.png",
                "img\\검색1.png"
            }, "검색");
            await Task.Delay(딜레이(키입력지연));

            GetSearchedImage(new string[] {
                "img\\방어구.png",
                "img\\방어구1.png"
            }, "방어구");
            await Task.Delay(딜레이(키입력지연));

            GetSearchedImage(new string[] {
                "img\\검색시작.png"
            }, "검색시작");
            await Task.Delay(딜레이(키입력지연));

            mouseclick(rand(198, 78, 14, 10));
            send_BS(20); // -- 여러번 입력할 땐 괄호 안에 횟수를 넣어주시면 되게끔 수정했습니다.


            Send("wbsl");
            send_enter();

            await Task.Delay(딜레이(키입력지연));

            좌표 임시좌표 = (UseImageSearch("img\\한영.png", "30"));
            if (임시좌표 != null)
            {
                한영키변경();
                await Task.Delay(딜레이(300));
                mouseclick(rand(205, 78, 14, 10));
                mouseclick(rand(205, 78, 14, 10));
                mouseclick(rand(205, 78, 14, 10));
                send_BS(20);
                Send("wbsl");
                send_enter(2);
            }
            send_enter();
            await Task.Delay(딜레이(키입력지연));
            화면전환대기("검색결과");
            GetSearchedImage(new string[] {
                "img\\검색결과.png"
            }, "검색결과", (coord) =>
            {
                coord.y += 60;
            });
            await Task.Delay(딜레이(키입력지연));
            do
                mouseclick(rand(좌표목록["검색결과"] as 좌표) as int[]);
            while (UseImageSearch(경로변환("구매하기"), "30") == null);
            GetSearchedImage(new string[] {
                "img\\구매하기.png"
            }, "구매하기");
            await Task.Delay(딜레이(키입력지연));

            GetSearchedImage(new string[] {
                "img\\판매.png",
                "img\\판매1.png"
            }, "판매");
            await Task.Delay(딜레이(키입력지연));

            GetSearchedImage(new string[] {
                "img\\판매대.png"
            }, "판매대");
            await Task.Delay(딜레이(키입력지연));

            GetSearchedImage(new string[] {
                "img\\빈슬롯1.png"
            }, "빈슬롯");
            await Task.Delay(딜레이(키입력지연));

            GetSearchedImage(new string[] {
                "img\\완료.png",
                "img\\완료1.png"
            }, "완료");
            await Task.Delay(딜레이(키입력지연));

            GetSearchedImage(new string[] {
                "img\\전체.png",
                "img\\전체1.png"
            }, "전체");
            await Task.Delay(딜레이(키입력지연));

            GetSearchedImage(new string[] {
                "img\\판매완료.png",
                "img\\판매완료1.png"
            }, "판매완료");
            await Task.Delay(딜레이(키입력지연));

            GetSearchedImage(new string[] {
                "img\\거래실패.png",
                "img\\거래실패1.png"
            }, "거래실패");
            await Task.Delay(딜레이(키입력지연));

            GetSearchedImage(new string[] {
                "img\\모두받기.png",
                "img\\모두받기1.png"
            }, "모두받기");
            await Task.Delay(딜레이(키입력지연));
            좌표입력("아이템이름", 212, 354, 16, 8);
            좌표입력("첫째옵션", 236, 494, 4, 7);
            좌표입력("둘째옵션", 236, 525, 4, 7);
            좌표입력("셋째옵션", 236, 555, 4, 7);
            좌표입력("가격", 236, 408, 4, 6);
            좌표입력("판매가격", 883, 227, 84, 9);
            좌표입력("판매등록", 870, 323, 128, 44);



            await Task.Delay(딜레이(300));
            랜덤결과값 = rand(좌표목록["검색"] as 좌표) as int[];
            mouseclick(랜덤결과값);
            await Task.Delay(딜레이(300));
            is_setting_done = true;
            //옵션설정();
        }
        private async Task sise_setting()
        {
            if (is_sise_setting_done)
                return;
            winactivate();
            좌표목록 = new Dictionary<string, 좌표>();
            좌표목록.Add("검색시작", new 좌표());
            좌표목록.Add("시세", new 좌표());

            GetSearchedImage(new string[] {
                "img\\시세.png",
                "img\\시세1.png"
            }, "시세");
            await Task.Delay(딜레이(300));
            GetSearchedImage(new string[] {
                "img\\검색시작.png"
            }, "검색시작");
            await Task.Delay(딜레이(300));

            is_sise_setting_done = true;
        }
        private Task 작업중인아쿠아틱만 = null;
        private List<Task> 작업중인Task = new List<Task>();
        public void 작업중인Task확인() => 작업중인Task = 작업중인Task.Where(t => t.Status == TaskStatus.Running).ToList();
        private async Task 장비구매()
        {
            while (true)
            {
                int[] 랜덤결과값;
                for (int cnt = 0; cnt < 180; cnt++)
                {
                    string 아이템이름 = ""; string 가격 = "";
                    DateTime 시작시간 = DateTime.Now;// 현재시간 찍기.
                    랜덤결과값 = rand(좌표목록["검색"] as 좌표) as int[];
                    mouseclick(랜덤결과값);
                    await Task.Delay(딜레이(키입력지연));

                    랜덤결과값 = rand(좌표목록["방어구"] as 좌표) as int[];
                    mouseclick(랜덤결과값);
                    await Task.Delay(딜레이(키입력지연));

                    랜덤결과값 = rand(좌표목록["검색시작"] as 좌표) as int[];
                    mouseclick(랜덤결과값);
                    await Task.Delay(딜레이(키입력지연));
                    mre.WaitOne();

                    send_enter(3); // Enter 3회
                    await Task.Delay(딜레이(20));

                    랜덤결과값 = rand(좌표목록["검색결과"] as 좌표) as int[];
                    mouseclick(랜덤결과값);
                    await Task.Delay(딜레이(키입력지연));

                    랜덤결과값 = rand(좌표목록["구매하기"] as 좌표) as int[];
                    mouseclick(랜덤결과값);
                    await Task.Delay(딜레이(키입력지연));

                    send_enter(3); // Enter 3회
                    await Task.Delay(딜레이(1000));

                    switch (cnt % 2)
                    {
                        case 0: 아이템이름 = "dmdcnrehls"; 가격 = "5555555"; break;
                        case 1: 아이템이름 = "dkzndkxlr"; 가격 = "5555555"; break;
                            //case 2: 아이템이름 = "vldzmqlc"; 가격 = "4444444"; break;
                    }
                    랜덤결과값 = rand(좌표목록["아이템이름"] as 좌표) as int[];
                    mouseclick(랜덤결과값);
                    await Task.Delay(딜레이(키입력지연));
                    send_BS(6);
                    Send(아이템이름);
                    await Task.Delay(딜레이(키입력지연));
                    랜덤결과값 = rand(좌표목록["가격"] as 좌표) as int[];
                    mouseclick(랜덤결과값);
                    await Task.Delay(딜레이(키입력지연));
                    send_BS(8);
                    Send(가격); await Task.Delay(딜레이(키입력지연));




                    winactivate();
                    //await Task.Delay(딜레이(int.Parse(textBox8.Text)));
                    winactivate();
                    //await Task.Delay(딜레이(int.Parse(textBox8.Text)));
                    mre.WaitOne();
                    TimeSpan 경과시간 = DateTime.Now - 시작시간;
                    /*if (경과시간.TotalSeconds < 5)
                        await Task.Delay(TimeSpan.FromMilliseconds(int.Parse(textBox8.Text)) - 경과시간);*/
                }
                정산();
            }
        }

        private 좌표 화면전환대기(string 이미지) => 화면전환대기(이미지, 0, 0, 1920, 1080);
        private 좌표 화면전환대기(string 이미지, int x1, int y1, int x2, int y2)
        {

            좌표 임시좌표 = (UseImageSearch("img\\" + 이미지 + ".png", x1, y1, x2, y2, "30"));
            while (임시좌표 == null)
            {
                mre.WaitOne();
                대기(딜레이(300));
                임시좌표 = (UseImageSearch("img\\" + 이미지 + ".png", x1, y1, x2, y2, "30"));

            }
            return 임시좌표;
        }
        private void 대기(int milliseconds = 키입력지연) => Task.Delay(milliseconds).GetAwaiter().GetResult();
        private void 정산()
        {
            long 수령후 = 0, 수령전 = 잔액확인(); StreamWriter writer;
            int[] 랜덤결과값;

            랜덤결과값 = rand(좌표목록["완료"] as 좌표) as int[];
            mouseclick(랜덤결과값);
            대기(딜레이(키입력지연));
            대기(딜레이(171));

            랜덤결과값 = rand(좌표목록["완료"] as 좌표) as int[];
            mouseclick(랜덤결과값);
            대기(딜레이(키입력지연));

            랜덤결과값 = rand(좌표목록["전체"] as 좌표) as int[];
            mouseclick(랜덤결과값);
            대기(딜레이(300));
            mre.WaitOne();


            if (UseImageSearch("img\\모두받기1.png", "30") == null)
            {
                return;
            }
            mouseclick(rand(좌표목록["모두받기"] as 좌표) as int[]);
            대기(딜레이(1000));
            send_enter();

            화면전환대기("수령중");
            send_enter();
            대기(딜레이(300));
            mre.WaitOne();
        }
        private void 옵션설정(string 올스탯, string 둘째옵션, string 옵션)
        {
            winactivate();
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                await setting();

            }).Wait();
            mouseclick(rand(좌표목록["검색"] as 좌표) as int[]);
            대기(딜레이(키입력지연));
            mouseclick(rand(좌표목록["첫째옵션"] as 좌표) as int[]);
            대기(딜레이(100));
            send_BS(4);
            Send(올스탯);
            mouseclick(rand(좌표목록["둘째옵션"] as 좌표) as int[]);
            대기(딜레이(100));
            send_BS(4);
            Send(둘째옵션);

            int cnt = 0;
            좌표 클릭대상 = null;
            if (!첫옵션올스탯)  // 검색탭 첫옵션 올스탯으로 설정
            {
                클릭대상 = 좌표목록["첫째옵션"];
                mouseclick(rand(클릭대상.x - 65, 클릭대상.y, 클릭대상.넓이, 클릭대상.높이));
                for (cnt = 0; cnt < 40; cnt++)
                {
                    send_위();
                }
                for (cnt = 0; cnt < 13; cnt++)
                {
                    send_아래();
                }
                send_enter();
                첫옵션올스탯 = true;
            }


            클릭대상 = 좌표목록["둘째옵션"];
            mouseclick(rand(클릭대상.x - 65, 클릭대상.y, 클릭대상.넓이, 클릭대상.높이));  // 검색할 둘째옵션 인자값대로 
            for (cnt = 0; cnt < 10; cnt++)
            {
                send_위();
            }
            switch (옵션)
            {
                case "마": send_아래(6); break;
                case "공": send_아래(5); break;
                case "럭": send_아래(4); break;
                case "인": send_아래(3); break;
                case "덱": send_아래(2); break;
                case "힘": send_아래(1); break;
            }
            send_enter();


        }

        private void 스크린샷(int x1, int x2, int y1, int y2)
        {
            winactivate(); 대기(300);
            DD_key(601, 1);
            DD_key(500, 1);
            DD_key(402, 1); 대기(100);
            DD_key(601, 2);
            DD_key(500, 2);
            DD_key(402, 2); 대기(100);

            드래그(x1, x2, y1, y2);
        }
        private Bitmap 스샷(int x, int y, int w, int h)
        {
            Bitmap b = new Bitmap(w, h);
            Graphics graphics = Graphics.FromImage(b);
            graphics.CopyFromScreen(x, y, 0, 0, b.Size);
            b = ConvertToFormat(b); // 스크린샷 찍고, 색변환
            return b;
        }
        void 드래그(int x1, int x2, int y1, int y2)
        {
            DD_mov(x1, y1); 대기(딜레이(키입력지연));
            DD_btn(1); 대기(딜레이(키입력지연));
            DD_mov(x2, y2); 대기(딜레이(키입력지연));
            DD_btn(2); 대기(딜레이(키입력지연));
        }

        void 반환위치확인()
        {
            mouseclick(rand(좌표목록["판매"] as 좌표) as int[]);
            대기(딜레이(300));
            GetSearchedImage(new string[] { "img\\빈슬롯1.png" }, "빈슬롯");
            mouseclick(rand(좌표목록["완료"] as 좌표) as int[]);
        }

        void 등록취소()
        {
            mouseclick(rand(좌표목록["판매"] as 좌표) as int[]);
            대기(딜레이(500));

            좌표 임시좌표 = (UseImageSearch("img\\x.png", "30"));
            while ((UseImageSearch("img\\x.png", "30")) != null)
            {
                mouseclick(rand(임시좌표));
                대기(700);
                mouseclick(rand(좌표목록["판매"] as 좌표) as int[]);
                대기(700);
            }
            mouseclick(rand(좌표목록["완료"] as 좌표) as int[]);
            대기(딜레이(300));
        }

        int 재등록()
        {
            string 할인된가격;
            string 서버별차감가격 = "0";
            int 재등록가능아이템갯수 = 10;
            if (is_setting_done == false)
                Task.Run(async () =>
                {
                    await setting();
                }).Wait();

            반환위치확인();
            mouseclick(rand(좌표목록["완료"] as 좌표) as int[]);
            대기(딜레이(1000));
            mouseclick(rand(좌표목록["거래실패"] as 좌표) as int[]);
            대기(딜레이(1000));
            //mouseclick(rand(125, 189, 6, 6));
            //대기(딜레이(1000));
            switch (현재서버)
            {
                /*                case ServerType.스카니아: 서버별차감가격 = textBox1.Text; break;
                                case ServerType.베라: 서버별차감가격 = textBox2.Text; break;
                                case ServerType.루나: 서버별차감가격 = textBox3.Text; break;
                                case ServerType.크로아: 서버별차감가격 = textBox4.Text; break;
                                case ServerType.엘리시움: 서버별차감가격 = textBox5.Text; break;
                                case ServerType.레드: 서버별차감가격 = textBox6.Text; break;
                                case ServerType.오로라: 서버별차감가격 = textBox7.Text; break;*/
            }


            for (int i = 10; i > 0; i--)
            {
                mre.WaitOne();
                좌표 임시좌표 = UseImageSearch("img\\재등록.png", "30");
                if (임시좌표 != null)
                {
                    mouseclick(rand(임시좌표));
                    long parsed = long.Parse(완료탭_가격());
                    long discounted = parsed - long.Parse(서버별차감가격);
                    if (discounted > 7000000) 할인된가격 = discounted.ToString();
                    else 할인된가격 = (parsed - 500000).ToString();
                    send_key(100); //취소
                    mouseclick(rand(UseImageSearch("img\\물품반환.png", "30")));

                    send_enter();// 반환되었는지 확인해야됨(안됐을경우 처리)
                    대기(딜레이(200));
                    send_enter(3);
                    대기(딜레이(200));
                    mouseclick(rand(좌표목록["판매"] as 좌표) as int[]);
                    대기(딜레이(200));
                    mouseclick(rand(좌표목록["빈슬롯"] as 좌표) as int[]);
                    대기(딜레이(200));
                    mouseclick(rand(좌표목록["판매대"] as 좌표) as int[]);
                    대기(딜레이(200));
                    mouseclick(rand(좌표목록["판매가격"] as 좌표) as int[]);
                    대기(딜레이(200));
                    Send(할인된가격);
                    대기(딜레이(200));
                    mouseclick(rand(좌표목록["판매등록"] as 좌표) as int[]); //옳은 가격에 올리려는지 이미지 서치 판단해야됨
                    대기(딜레이(100));
                    send_enter(4);
                    대기(딜레이(1500));
                    mouseclick(rand(좌표목록["완료"] as 좌표) as int[]);
                    재등록가능아이템갯수 = i;
                }
                else
                {
                    mouseclick(rand(547, 711, 5, 5)); 대기(딜레이(1000));
                    임시좌표 = (UseImageSearch("img\\재등록.png", "30"));
                    if (임시좌표 != null)
                    {
                        mouseclick(rand(임시좌표));
                        long parsed = long.Parse(완료탭_가격());
                        long discounted = parsed - long.Parse(서버별차감가격);
                        if (discounted > 7000000) 할인된가격 = discounted.ToString();
                        else 할인된가격 = parsed.ToString();
                        send_key(100); //취소

                        mouseclick(rand((UseImageSearch("img\\물품반환.png", "30"))));

                        send_enter();// 반환되었는지 확인해야됨(안됐을경우 처리)
                        대기(딜레이(100));
                        send_enter();
                        대기(딜레이(100));
                        mouseclick(rand(좌표목록["판매"] as 좌표) as int[]);
                        대기(딜레이(500));
                        mouseclick(rand(좌표목록["빈슬롯"] as 좌표) as int[]);
                        대기(딜레이(100));
                        mouseclick(rand(좌표목록["판매대"] as 좌표) as int[]);
                        대기(딜레이(100));
                        mouseclick(rand(좌표목록["판매가격"] as 좌표) as int[]);
                        대기(딜레이(100));
                        Send(할인된가격);
                        대기(딜레이(400));
                        mouseclick(rand((UseImageSearch("img\\판매등록.png", "30")))); //옳은 가격에 올리려는지 이미지 서치 판단해야됨
                        대기(딜레이(100));
                        send_enter(3);
                        대기(딜레이(1500));
                        mouseclick(rand(좌표목록["완료"] as 좌표) as int[]);
                        재등록가능아이템갯수 = i;
                    }
                    else
                        break;
                }
            }

            return 재등록가능아이템갯수;
        }
        void 판매등록(int 판매가능갯수, ServerType 서버이름)
        {
            for (int i = 0; i < 판매가능갯수; i++)
            {
                mre.WaitOne();
                if (is_setting_done == false) // 좌표등록안되어있으면 하세요!
                    Task.Run(async () => await setting()).Wait(); // -- 코드가 한줄 일 땐 이렇게 줄일 수 있습니다.


                mouseclick(rand(좌표목록["판매"] as 좌표) as int[]); //판매탭 가서
                대기(딜레이(100));

                좌표 임시좌표 = UseImageSearch("img\\아쿠아이콘.png", 73, 152, 661, 372);
                if (임시좌표 != null)
                {
                    mousemove(rand(임시좌표));
                }
                else
                    break;
                대기(딜레이(100));
                int[] 옵션 = 판매탭_옵션확인(); mre.WaitOne();            //아이템 옵션 받아오고
                대기(딜레이(200));
                string 가격 = 시세검색(옵션, 서버이름); mre.WaitOne();           //아이템 시세 검색하고
                do
                {
                    mouseclick(rand(좌표목록["판매"] as 좌표) as int[]); //판매탭 돌아와서
                    대기(딜레이(100));
                    mouseclick(rand(임시좌표)); // 아쿠아틱 집어다가
                    대기(딜레이(100));
                    mouseclick(rand(좌표목록["판매대"] as 좌표) as int[]);//판매대 위에 올리세요.
                    대기(딜레이(100));
                    mouseclick(rand(좌표목록["판매가격"] as 좌표) as int[]);//판매가격 커서 두고
                    Send(가격);                                           // 가격 입력하세요
                    대기(딜레이(400));
                    mre.WaitOne();
                }
                while (UseImageSearch("img\\판매등록.png", "30") == null);

                mouseclick(rand(UseImageSearch("img\\판매등록.png", "30"))); //옳은 가격에 올리려는지 이미지 서치 판단해야됨
                대기(딜레이(100));
                send_enter();
                대기(딜레이(100));
                send_enter();
                대기(딜레이(100));
                send_enter();
            }
        }

        string 시세검색(int[] 옵션, ServerType 서버이름)
        {
            string[] 시세 = new string[10]; // 시세는 나중에 데이터베이스에 넣을거에요.
            int[] 평균값 = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            string 추가옵션 = "힘";
            string 최종가격; int 유효옵션 = 옵션[0] + 옵션[1] + 옵션[2] + 옵션[3] + 옵션[4] + 옵션[5];
            int 개꿀1 = 0; int 개꿀2 = 0; int 임시;
            int 가격수집갯수 = 3;


            mouseclick(rand(좌표목록["검색"] as 좌표) as int[]); // 검색탭 가서
            mre.WaitOne();
            if (가격지웠니 == false)
            {
                mouseclick(rand(좌표목록["가격"] as 좌표) as int[]); // 가격란에 입력된거 지우기
                대기(딜레이(100));
                send_BS(10);
                가격지웠니 = true;
            }
            mre.WaitOne();
            if (유효옵션 == 0 && (
                    (옵션[6] == 5 && 시세정보.가져오기(서버이름).올5 == "")
                    || (옵션[6] == 6 && 시세정보.가져오기(서버이름).올6 == "")
                    || (옵션[6] == 7 && 시세정보.가져오기(서버이름).올7 == "")
               ))   // 아이템 스텟이 추가옵션없고 올스텟만붙었고 올스탯 저장평균값이 없으면 평균값 구하기
            {
                옵션설정(옵션[6].ToString(), "6", "힘"); // 올스탯 수치 넣고 , 둘째옵션 힘으로 6만큼 넣은상태로 검색옵션 설정하기
                mouseclick(rand(좌표목록["검색시작"] as 좌표) as int[]);
                send_enter(3);

                대기(딜레이(300));
                좌표 임시좌표 = (UseImageSearch("img\\확인.png", "30"));  // 결과 너무 많이 떴다고 나오면 팝업창 확인누르기
                if (임시좌표 != null)
                {
                    mouseclick(rand(임시좌표));
                }
                대기(딜레이(300));

                시세 = 검색탭_가격(가격수집갯수, 옵션[6]);                             // 시세 3개 검색해서 배열에 넣기
                //평균값[0] = (int.Parse(시세[0]) + int.Parse(시세[1]) + int.Parse(시세[2])) / 3;
                평균값[0] = (int)시세.Take(가격수집갯수).Select(가격 => int.Parse(가격)).Average();
                if (평균값[0].ToString().Length == 7)
                    최종가격 = (평균값[0].ToString()).Substring(0, 3);
                else if (평균값[0].ToString().Length == 8)
                    최종가격 = (평균값[0].ToString()).Substring(0, 4);
                else
                {
                    MessageBox.Show("시세 평균값 계산 오류\r받은평균값 :" + 평균값[0]);
                    최종가격 = "999999999999";
                }

                switch (옵션[6])
                {
                    case 5: 시세정보.가져오기(서버이름).올5 = 최종가격; break;
                    case 6: 시세정보.가져오기(서버이름).올6 = 최종가격; break;
                    case 7: 시세정보.가져오기(서버이름).올7 = 최종가격; break;
                }
                //if (옵션[6] == 5) 올스탯5 = 최종가격;
                //if (옵션[6] == 6) 올스탯6 = 최종가격;
                //if (옵션[6] == 7) 올스탯7 = 최종가격;
                최종가격 += "0000";
                return 최종가격;
            }
            //else if (옵션[0] + 옵션[1] + 옵션[2] + 옵션[3] + 옵션[4] + 옵션[5] == 0)
            // 옵션[0] ~ 옵션[5]까지 6개의 총합
            else if (옵션.Take(6).Sum() == 0) // 옵션[0] ~ 옵션[5]까지 6개의 총합
            {
                if (옵션[6] == 5)
                    최종가격 = 시세정보.가져오기(서버이름).올5;
                else if (옵션[6] == 6)
                    최종가격 = 시세정보.가져오기(서버이름).올6;
                else
                    최종가격 = 시세정보.가져오기(서버이름).올7;

                return 최종가격 + "0000";
            }
            //else if (옵션[0] + 옵션[1] + 옵션[2] + 옵션[3] == 0 && (옵션[4] != 0 || 옵션[5] != 0)) //공격력이나 마력만 붙었을 때,검색하세요
            //옵션[0] ~ 옵션[3]까지 4개의 총합
            else if (옵션.Take(4).Sum() == 0 && (옵션[4] != 0 || 옵션[5] != 0))
            {
                for (int i = 4; i < 6; i++)
                {
                    mre.WaitOne();

                    if (옵션[i] != 0)  // 검색하려는 옵션이 6이 아니면 검색하세요. 
                    {
                        switch (i)
                        {
                            case 4: 추가옵션 = "공"; break;
                            case 5: 추가옵션 = "마"; break;
                        }
                        임시 = 옵션[i] + 1; //1은 마력 혹은 공격력 기본옵션 
                        if (임시 == 7 || 임시 == 8)                                 // 너무 오래검색하게되니까 임시로 변경시킴
                            임시 = 6;
                        옵션설정(옵션[6].ToString(), 임시.ToString(), 추가옵션);
                        mouseclick(rand(좌표목록["검색시작"] as 좌표) as int[]);
                        send_enter(3);
                        DateTime 시작시간 = DateTime.Now;// 현재시간 찍기.
                        대기(딜레이(300));
                        좌표 임시좌표 = (UseImageSearch("img\\확인.png", "30"));
                        if (임시좌표 != null)
                        {
                            mouseclick(rand(임시좌표));
                        }
                        시세 = 검색탭_가격(2/*가격수집갯수*/, 추가옵션, 임시, 옵션[6]);
                        평균값[i - 4] = (int)시세.Take(2/*가격수집갯수*/).Select(가격 => int.Parse(가격)).Average();
                        //대기(딜레이(2000));

                        TimeSpan 경과시간 = DateTime.Now - 시작시간;
                        /*                        if (경과시간.TotalSeconds < 5)
                                                    대기((int)(TimeSpan.FromMilliseconds(int.Parse(textBox8.Text)) - 경과시간).TotalMilliseconds);*/
                    }
                }

                //int max = 평균값[0];
                //if (max < 평균값[1])
                //    max = 평균값[1];
                int max = new[] { 평균값[0], 평균값[1] }.Max(); // 둘 중 큰 값이 max에 들어감
                if (max.ToString().Length == 7)
                    최종가격 = (max.ToString()).Substring(0, 3);
                else if (max.ToString().Length == 8)
                    최종가격 = (max.ToString()).Substring(0, 4);
                else
                    최종가격 = (max.ToString()).Substring(0, 5);

                return 최종가격 + "0000";

            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    mre.WaitOne();
                    if (옵션[i] != 0)  // 검색하려는 옵션이 6이 아니면 검색하세요. 
                    {
                        switch (i)
                        {
                            case 0: 추가옵션 = "힘"; break;
                            case 1: 추가옵션 = "덱"; break;
                            case 2: 추가옵션 = "인"; break;
                            case 3: 추가옵션 = "럭"; break;
                        }
                        개꿀1 = 옵션[4] * 3;   //만약 공격력이 붙었으면 
                        개꿀2 = 옵션[5] * 3;   //만약 마력이 붙었으면 

                        if (i != 2)
                            임시 = 옵션[i] + 개꿀1 + 6; //6은 기본옵션 공격력이랑 스텟이랑 같이 붙었을때
                        else
                            임시 = 옵션[i] + 개꿀2 + 6; //6은 기본옵션 마력이랑 스텟이랑 같이 붙었을 때.

                        string 검색옵션 = "올" + 옵션[6].ToString() + 추가옵션 + 옵션[i].ToString();
                        if (시세정보.가져오기(서버이름).스탯가격가져오기(검색옵션) == "")
                        {
                            옵션설정(옵션[6].ToString(), 임시.ToString(), 추가옵션);
                            mouseclick(rand(좌표목록["검색시작"] as 좌표) as int[]);
                            send_enter(3);
                            DateTime 시작시간 = DateTime.Now;// 현재시간 찍기.
                            대기(딜레이(300));
                            좌표 임시좌표 = (UseImageSearch("img\\확인.png", "30"));
                            if (임시좌표 != null)
                            {
                                mouseclick(rand(임시좌표));
                            }
                            대기(딜레이(300));
                            시세 = 검색탭_가격(가격수집갯수, 추가옵션, 임시, 옵션[6]);
                            평균값[i] = (int)시세.Take(가격수집갯수).Select(가격 => int.Parse(가격)).Average();
                            시세정보.가져오기(서버이름).스탯가격[검색옵션] = 평균값[i].ToString();
                            //대기(딜레이(2000));
                            TimeSpan 경과시간 = DateTime.Now - 시작시간;
                            /*                            if (경과시간.TotalSeconds < 5)
                                                            대기((int)(TimeSpan.FromMilliseconds(int.Parse(textBox8.Text)) - 경과시간).TotalMilliseconds);*/

                        }
                        else
                        {
                            평균값[i] = int.Parse(시세정보.가져오기(서버이름).스탯가격가져오기(검색옵션));
                        }
                    }
                }

                int max = 평균값.Max();

                switch (max.ToString().Length)
                {
                    case 7:
                        최종가격 = (max.ToString()).Substring(0, 3);
                        break;
                    case 8:
                        최종가격 = (max.ToString()).Substring(0, 4);
                        break;
                    default:
                        최종가격 = (max.ToString()).Substring(0, 5);
                        break;
                }
                return 최종가격 + "0000";
            }

        }
        void 경매장입장()
        {
            int count = 0;
            //좌표 임시좌표;
            //화면전환대기("접속완료확인");
            좌표 임시좌표 = (UseImageSearch("img\\접속완료확인.png", "30"));
            while (임시좌표 == null)
            {
                대기(딜레이(1000));
                임시좌표 = (UseImageSearch("img\\접속완료확인.png", "30"));
                if (count == 100)
                {
                    실행확인();
                    Task.Run(async () =>
                    {
                        await 로그인(현재ID);
                    }).Wait();
                    서버입장(현재서버);
                    캐릭입장(현재서버);

                    count = 0;
                }
                count++;
            }
            대기(딜레이(3000));

            do
            {
                대기(딜레이(1000));
                임시좌표 = (UseImageSearch("img\\메이플옥션.png", "30"));
                count++;
                if (count % 3 == 0)
                {
                    mouseclick(rand(864, 117, 19, 17)); // 팝업 x창 지우는 것
                    send_key(311);
                    대기(딜레이(1000));
                    send_아래(4);
                    send_enter();
                    대기(딜레이(4000));

                }
                if (count == 100)
                {
                    실행확인();
                    Task.Run(async () =>
                    {
                        await 로그인(현재ID);
                    }).Wait();
                    서버입장(현재서버);
                    캐릭입장(현재서버);

                    count = 0;
                }
            } while (임시좌표 == null);
            count = 0;
        }
        void 게임퇴장()
        {
            ///////////////////////////// 여기 재실행 코드 입력해야함
            int count = 0;
            좌표 임시좌표 = (UseImageSearch("img\\접속완료확인.png", "30"));
            while (임시좌표 == null)
            {
                대기(딜레이(1000));
                임시좌표 = (UseImageSearch("img\\접속완료확인.png", "30"));
                if (count == 100)
                {
                    실행확인();
                    Task.Run(async () =>
                    {
                        await 로그인(현재ID);
                    }).Wait();
                    서버입장(현재서버);
                    캐릭입장(현재서버);
                    대기(딜레이(3000));
                    count = 0;
                }
                count++;
            }


            send_key(100);
            대기(딜레이(키입력지연));
            send_위();
            대기(딜레이(키입력지연));
            send_enter();
            대기(딜레이(1000));
            send_enter();
        }
        void 서버퇴장()
        {
            좌표 임시좌표;
            임시좌표 = 화면전환대기("이전으로");
            대기(딜레이(300));
            mouseclick(rand(임시좌표)); mouseclick(rand(임시좌표));


        }
        void 수익기록(long Income, long Total)
        {
            StreamWriter writer;
            var FileName = $"수익산출표-{DateTime.Today.ToString("yy-MM")}.csv";
            if (File.Exists(FileName) == false)
            {
                using (writer = new StreamWriter(File.Open(FileName, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8))
                {
                    writer.WriteLine($"시간,아이디,서버,수입,보유메소");
                }
            }
            using (writer = new StreamWriter(File.Open(FileName, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8))
            {
                var Row = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ")}," +
                    $"{현재ID},{현재서버},{Income},{Total}";
                writer.WriteLine(Row);
            }
            //writer = File.AppendText("수익산출표.txt");
            //writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss \t아이디:\t" + 현재ID + "\t서버:\t" + 현재서버 + "\t수입:\t" + 수령후 + "\t보유메소:\t" + 수령전));
            //writer.Close(); 
            데이터베이스.추가(new IncomeReport
            {
                Server = 현재서버,
                AccountName = 현재ID.ToString(),
                Income = Income,
                Total = Total
            });
        }
        void 판매완료()
        {
            Task.Run(async () =>
            {
                await setting();

            }).Wait();
            long 수령후 = 0, 수령전 = 잔액확인(), 실수익 = 0;
            mouseclick(rand(좌표목록["완료"] as 좌표) as int[]);
            대기(딜레이(1000));
            mouseclick(rand(좌표목록["판매완료"] as 좌표) as int[]);
            대기(딜레이(1000));
            if (UseImageSearch("img\\모두받기1.png", "30") == null)
            {
                수익기록(실수익, 수령전);
                return;
            }

            mouseclick(rand(좌표목록["모두받기"] as 좌표) as int[]);
            대기(딜레이(1000));
            send_enter();

            화면전환대기("수령중");
            send_enter();

            수령후 = 잔액확인(); 실수익 = 수령후 - 수령전;
            수익기록(실수익, 수령후);
            //writer = File.AppendText("수익산출표.txt");
            //writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss \t아이디:\t" + 현재ID + "\t서버:\t" + 현재서버 + "\t수입:\t" + 수령전 + "\t보유메소:\t" + 수령후));
            //writer.Close();

        }
        long 잔액확인()
        {
            mouseclick(rand(좌표목록["판매"] as 좌표) as int[]);
            mouseclick(rand(좌표목록["완료"] as 좌표) as int[]);
            Image image = Image.FromFile("img\\가격0.png");

            int 메소좌표x = 939, 메소좌표y = 78, w = image.Width - 1, h = image.Height - 1, j = 1, i = 0;
            int 간격 = 0; long result = 0;
            while (true)
            {
                for (i = 0; i < 10; i++)
                {
                    if (UseImageSearch("img\\가격" + i + ".png", 메소좌표x - 간격, 메소좌표y, 메소좌표x - 간격 + w, 메소좌표y + h) != null)
                    {
                        result += ((long)Math.Pow(10, j - 1) * i);
                        break;
                    }
                }
                if (i == 10)
                {
                    return result;
                }
                if (j % 3 == 0)
                    간격 += 11;
                else 간격 += 8;
                j++;
            }
        }

        Task 로그아웃()
        {
            좌표 임시좌표 = (UseImageSearch("img\\처음으로확인.png", "30"));
            while (임시좌표 == null)
            {
                mouseclick(rand(4, 709, 70, 20));
                mre.WaitOne();
                임시좌표 = (UseImageSearch("img\\처음으로확인.png", "30"));
                대기(딜레이(1000));
            }
            mouseclick(rand(임시좌표));

            대기(딜레이(5000));
            return Task.CompletedTask;
        }
        void 경매장퇴장()
        {
            mre.WaitOne();
            int count = 0;
            //mouseclick(rand(UseImageSearch("img\\나가기.png", "30")));
            좌표 임시좌표 = (UseImageSearch("img\\나가기.png", "30"));
            while (임시좌표 == null)
            {
                대기(딜레이(1000));
                임시좌표 = (UseImageSearch("img\\나가기.png", "30"));
                if (count == 100)
                {
                    실행확인();
                    Task.Run(async () =>
                    {
                        await 로그인(현재ID);
                    }).Wait();
                    서버입장(현재서버);
                    캐릭입장(현재서버);
                    경매장입장();
                    count = 0;
                }
                count++;
            }
            mouseclick(rand(UseImageSearch("img\\나가기.png", "30")));
            대기(딜레이(3000));

        }
        void 서버입장(ServerType 서버이름)
        {
            현재서버 = (ServerType)서버이름;
            int i = 0; int count = 0;
            switch (서버이름)
            { // 예티x핑크빈 서버위치 변경됨.
                case ServerType.스카니아: i = 0 + 2; break;
                case ServerType.베라: i = 1 + 2; break;
                case ServerType.루나: i = 2 + 2; break;
                case ServerType.크로아: i = 4 + 2; break;
                case ServerType.엘리시움: i = 6 + 2; break;
                case ServerType.레드: i = 8 + 2; break;
                case ServerType.오로라: i = 9 + 2; break;
            }
            mouseclick(rand(903, 62 + (i * 33), 79, 22));
            대기(딜레이(1000));
            while ((UseImageSearch("img\\선택채널가기.png", "30")) == null)
            {
                mre.WaitOne();
                대기(딜레이(1000));
                mouseclick(rand(903, 62 + (i * 33), 79, 22));
                if (count == 500)
                {
                    로그인(현재ID);
                    서버입장(현재서버);
                    break;
                }
                count++;
            }
            count = 0;
            send_enter();
            대기(딜레이(3000));
            while ((UseImageSearch("img\\서버입장확인.png", "30")) == null) // 캐릭입장창이 나올때까지 엔터누르세요
            {
                send_enter();
                대기(딜레이(3000));
                if (count == 500)
                {
                    로그인(현재ID);
                    서버입장(현재서버);
                    break;
                }
                count++;

            }
        }
        void 캐릭입장(ServerType 서버이름)
        {
            int count = 0;
            while ((UseImageSearch("img\\서버입장확인.png", "30")) != null)  // 캐릭입장창에서 "게임시작"이미지가 보이면 계속 엔터누르세요
            {
                send_enter();
                대기(딜레이(3000));
                if (count == 500)
                {
                    로그인(현재ID);
                    서버입장(현재서버);
                    캐릭입장(현재서버);
                    break;
                }
                count++;
                mre.WaitOne();
            }
        }
        Task 아이디변경(int i)
        {
            winactivate();
            대기(딜레이(1000));
            mouseclick(rand(522, 355, 21, 16)); // 아이디창 클릭
            send_BS(30);
            if (i == 11)
            {
                Send("bigshot11");
                if (UseImageSearch("img\\빅샷한글.png", "30") != null)
                {
                    한영키변경();
                    send_BS(12);
                    Send("bigshot11");
                }
                Send(조합키.Shift, "2");
                Send("naverDcom");
            }
            if (i == 75)
            {
                Send("abcd9575");
                if (UseImageSearch("img\\abcd한글.png", "30") != null)
                {
                    한영키변경();
                    send_BS(12);
                    Send("abcd9575");
                }
                Send(조합키.Shift, "2");
                Send("daumDnet");
            }
            if (i == 95)
            {
                Send("abcd95751");
                if (UseImageSearch("img\\abcd한글.png", "30") != null)
                {
                    한영키변경();
                    send_BS(12);
                    Send("abcd95751");
                }
                Send(조합키.Shift, "2");
                Send("gmailDcom");
            }


            return Task.CompletedTask;
        }
        Task 네트워크주소변경(int i)
        {
            DD_key(601, 1); Send("r"); DD_key(601, 2); 대기(딜레이(500)); send_enter(); 대기(딜레이(1000));
            좌표 임시좌표 = (UseImageSearch("img\\네트워크어댑터.png", "30"));
            while (임시좌표 == null)
                임시좌표 = (UseImageSearch("img\\네트워크어댑터.png", "30"));
            mouseclick(임시좌표); 대기(딜레이(키입력지연)); send_오른쪽(); 대기(딜레이(키입력지연));
            임시좌표 = (UseImageSearch("img\\realtek.png", "30"));
            while (임시좌표 == null)
                임시좌표 = (UseImageSearch("img\\realtek.png", "30"));
            mouseclick(임시좌표); 대기(딜레이(키입력지연)); send_enter(); 대기(딜레이(500)); DD_key(600, 1); send_key(300); DD_key(600, 2); 대기(딜레이(500));
            mousemove(354, 364); DD_whl(2); 대기(500);
            임시좌표 = (UseImageSearch("img\\네트워크주소.png", "30"));
            while (임시좌표 == null)
                임시좌표 = (UseImageSearch("img\\네트워크주소.png", "30"));
            mouseclick(임시좌표); 대기(딜레이(500));
            if (i == 0)
            {
                mouseclick(501, 278); 대기(딜레이(500));
                send_enter(); 대기(딜레이(1000));
                mouseclick(836, 58); 대기(딜레이(1000));

                /*mouseclick(617, 245); 대기(딜레이(500));
                send_BS(14); Send("000000000003"); 대기(딜레이(500)); send_enter(); 대기(딜레이(3000)); mouseclick(836, 58); 대기(딜레이(1000));*/
            }
            if (i == 1)
            {
                mouseclick(617, 245); 대기(딜레이(500));
                send_BS(14); Send("000000000007"); 대기(딜레이(500)); send_enter(); 대기(딜레이(3000)); mouseclick(836, 58); 대기(딜레이(1000));
            }
            if (i == 2)
            {
                mouseclick(617, 245); 대기(딜레이(500));
                send_BS(14); Send("000000000010"); 대기(딜레이(500)); send_enter(); 대기(딜레이(3000)); mouseclick(836, 58); 대기(딜레이(1000));
            }
            if (i == 3)
            {
                mouseclick(617, 245); 대기(딜레이(500));
                send_BS(14); Send("000000000008"); 대기(딜레이(500)); send_enter(); 대기(딜레이(3000)); mouseclick(836, 58); 대기(딜레이(1000));
            }
            대기(딜레이(100000));
            return Task.CompletedTask;
        }
        static System.Drawing.Bitmap CopyToBpp(System.Drawing.Bitmap b, int bpp)
        {
            if (bpp != 1 && bpp != 8) throw new System.ArgumentException("1 or 8", "bpp");

            // Plan: built into Windows GDI is the ability to convert
            // bitmaps from one format to another. Most of the time, this
            // job is actually done by the graphics hardware accelerator card
            // and so is extremely fast. The rest of the time, the job is done by
            // very fast native code.
            // We will call into this GDI functionality from C#. Our plan:
            // (1) Convert our Bitmap into a GDI hbitmap (ie. copy unmanaged->managed)
            // (2) Create a GDI monochrome hbitmap
            // (3) Use GDI "BitBlt" function to copy from hbitmap into monochrome (as above)
            // (4) Convert the monochrone hbitmap into a Bitmap (ie. copy unmanaged->managed)

            int w = b.Width, h = b.Height;
            IntPtr hbm = b.GetHbitmap(); // this is step (1)
                                         //
                                         // Step (2): create the monochrome bitmap.
                                         // "BITMAPINFO" is an interop-struct which we define below.
                                         // In GDI terms, it's a BITMAPHEADERINFO followed by an array of two RGBQUADs
            BITMAPINFO bmi = new BITMAPINFO();
            bmi.biSize = 40;  // the size of the BITMAPHEADERINFO struct
            bmi.biWidth = w;
            bmi.biHeight = h;
            bmi.biPlanes = 1; // "planes" are confusing. We always use just 1. Read MSDN for more info.
            bmi.biBitCount = (short)bpp; // ie. 1bpp or 8bpp
            bmi.biCompression = BI_RGB; // ie. the pixels in our RGBQUAD table are stored as RGBs, not palette indexes
            bmi.biSizeImage = (uint)(((w + 7) & 0xFFFFFFF8) * h / 8);
            bmi.biXPelsPerMeter = 1000000; // not really important
            bmi.biYPelsPerMeter = 1000000; // not really important
                                           // Now for the colour table.
            uint ncols = (uint)1 << bpp; // 2 colours for 1bpp; 256 colours for 8bpp
            bmi.biClrUsed = ncols;
            bmi.biClrImportant = ncols;
            bmi.cols = new uint[256]; // The structure always has fixed size 256, even if we end up using fewer colours
            if (bpp == 1) { bmi.cols[0] = MAKERGB(0, 0, 0); bmi.cols[1] = MAKERGB(255, 255, 255); }
            else { for (int i = 0; i < ncols; i++) bmi.cols[i] = MAKERGB(i, i, i); }
            // For 8bpp we've created an palette with just greyscale colours.
            // You can set up any palette you want here. Here are some possibilities:
            // greyscale: for (int i=0; i<256; i++) bmi.cols[i]=MAKERGB(i,i,i);
            // rainbow: bmi.biClrUsed=216; bmi.biClrImportant=216; int[] colv=new int[6]{0,51,102,153,204,255};
            //          for (int i=0; i<216; i++) bmi.cols[i]=MAKERGB(colv[i/36],colv[(i/6)%6],colv[i%6]);
            // optimal: a difficult topic: http://en.wikipedia.org/wiki/Color_quantization
            // 
            // Now create the indexed bitmap "hbm0"
            IntPtr bits0; // not used for our purposes. It returns a pointer to the raw bits that make up the bitmap.
            IntPtr hbm0 = CreateDIBSection(IntPtr.Zero, ref bmi, DIB_RGB_COLORS, out bits0, IntPtr.Zero, 0);
            //
            // Step (3): use GDI's BitBlt function to copy from original hbitmap into monocrhome bitmap
            // GDI programming is kind of confusing... nb. The GDI equivalent of "Graphics" is called a "DC".
            IntPtr sdc = GetDC(IntPtr.Zero);       // First we obtain the DC for the screen
                                                   // Next, create a DC for the original hbitmap
            IntPtr hdc = CreateCompatibleDC(sdc); SelectObject(hdc, hbm);
            // and create a DC for the monochrome hbitmap
            IntPtr hdc0 = CreateCompatibleDC(sdc); SelectObject(hdc0, hbm0);
            // Now we can do the BitBlt:
            BitBlt(hdc0, 0, 0, w, h, hdc, 0, 0, SRCCOPY);
            // Step (4): convert this monochrome hbitmap back into a Bitmap:
            System.Drawing.Bitmap b0 = System.Drawing.Bitmap.FromHbitmap(hbm0);
            //
            // Finally some cleanup.
            DeleteDC(hdc);
            DeleteDC(hdc0);
            ReleaseDC(IntPtr.Zero, sdc);
            DeleteObject(hbm);
            DeleteObject(hbm0);
            //
            return b0;
        }
        Task 메이플실행()
        {
            서버끊김();
            대기(딜레이(1000));
            send_key(601);
            좌표 임시좌표 = (UseImageSearch("img\\메이플아이콘.png", "30"));
            while (임시좌표 == null)
                임시좌표 = (UseImageSearch("img\\메이플아이콘.png", "30"));
            mouseclick(임시좌표); 대기(딜레이(3000));
            임시좌표 = (UseImageSearch("img\\게임실행.png", "30"));
            while (임시좌표 == null)
                임시좌표 = (UseImageSearch("img\\게임실행.png", "30"));
            임시좌표.x = 임시좌표.x + (임시좌표.넓이 / 2); 임시좌표.y = 임시좌표.y + (임시좌표.높이 / 2);
            mouseclick(임시좌표); 대기(딜레이(3000));

            임시좌표 = (UseImageSearch("img\\로그인창확인.png", "30"));
            while (임시좌표 == null)
            {
                임시좌표 = (UseImageSearch("img\\로그인창확인.png", "30"));
            }
            return Task.CompletedTask;
        }
        Task 서버끊김()
        {
            좌표 임시좌표 = UseImageSearch("img\\서버끊김1.png", "30");
            if (임시좌표 != null) mouseclick(임시좌표);
            임시좌표 = UseImageSearch("img\\서버끊김2.png", "30");
            if (임시좌표 != null) mouseclick(임시좌표);
            return Task.CompletedTask;
        }

        Task 로그인(int 아이디)
        {
            현재ID = 아이디;
            winactivate();
            int count = 0;
            string ID = "";
            switch (아이디)
            {
                //case에 명시된 숫자와 맞아 떨어지는 경우에만 ID 뒤에 숫자를 넣어줍니다
                //case 에 숫자만 추가하면 추가된 숫자를 뒤에 붙여줍니다.
                case 77:
                case 78:
                case 79:
                case 80:
                case 81:
                case 82:
                case 83:
                    현재계정 = 75; ID = $"bigshot{아이디}";
                    break;
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                    현재계정 = 11; ID = $"bigshot{아이디}";
                    break;
                case 975:
                    현재계정 = 95; ID = $"bigshot{아이디}";
                    break;

            }
            대기(딜레이(500));
            mouseclick(rand(503, 398, 52, 19));// 비밀번호창 클릭
            대기(딜레이(200));
            Send("rlselsurt1");
            Send(조합키.Shift, "3");
            대기(딜레이(1000));
            send_enter();


            화면전환대기("아이디선택창");

            while (true)
            {

                좌표 임시좌표 = (UseImageSearch("img\\" + ID + ".png", "30"));
                if (임시좌표 != null)
                {
                    mouseclick(rand(UseImageSearch("img\\" + ID + ".png", "30")));
                    send_enter();
                    대기(딜레이(3000));
                    break;
                }
                else
                {
                    mre.WaitOne();
                    if (count++ < 5)
                    {
                        send_아래(2);
                    }
                    else
                    {
                        send_위(2);
                        if (count == 11)
                            count = 0;
                    }
                }
                대기(딜레이(1000));
            }
            return Task.CompletedTask;
        }
        Task 판매등록루틴(ServerType 서버이름)
        {
            서버입장(서버이름);
            캐릭입장(서버이름);
            경매장입장();
            판매완료();
            등록취소();
            int 판매가능아이템수 = 재등록();
            판매등록(판매가능아이템수, 서버이름);
            //정산();
            //판매등록(10, 서버이름);
            경매장퇴장();
            게임퇴장();
            서버퇴장();
            return Task.CompletedTask;
        }
        Task 시세루틴(ServerType 서버이름)
        {
            서버입장(서버이름);
            캐릭입장(서버이름);
            경매장입장();
            시세탭_가격분석();
            경매장퇴장();
            게임퇴장();
            서버퇴장();
            return Task.CompletedTask;
        }

        void 특가찾기(ref Bitmap price, string 이미지이름, ref int breaker, ref 좌표 특가좌표, ref Bitmap 특가아이디, ref Bitmap ID_new, ref Bitmap id1, ref Bitmap id2, ref Bitmap id3, ref int 실패, ref 좌표 itemCoord)
        {
            List<좌표> 특가;
            SoundPlayer player = new SoundPlayer("노크.wav");
            특가 = imageMatching(price, ConvertToFormat(new Bitmap(경로변환(이미지이름))));
            if (특가.Count() > 0 && breaker == 0)
            {
                foreach (var 특 in 특가)
                {
                    if (특.x > 90 && 특.x < 400)
                    {
                        특가좌표.x += 240; 특가아이디 = ConvertToFormat(crop(ID_new, 236, 0, 90, 33));
                    }
                    if (특.x > 400)
                    {
                        특가좌표.x += 480; 특가아이디 = ConvertToFormat(crop(ID_new, 471, 0, 90, 33));
                    }
                }
                if (imageMatching(특가아이디, id1).Count() + imageMatching(특가아이디, id2).Count() + imageMatching(특가아이디, id3).Count() == 0)
                {
                    mouseclick(특가좌표); player.Play();
                    breaker++;
                    do     //아쿠아틱 옵션 팝업 안보이면 값 받을때까지 반복
                        itemCoord = (UseImageSearch(경로변환("이더마크2"), 1220, 205, 1280 + 18, 205 + 23, "30"));
                    while (itemCoord == null);//구매하기()
                    mouseclick(1400, 225);
                    do     //아쿠아틱 옵션 팝업 안보이면 값 받을때까지 반복
                    {
                        itemCoord = (UseImageSearch(경로변환("confirm transaction"), 1557, 40, 1557 + 169, 40 + 38, "30"));
                        if (UseImageSearch(경로변환("실패"), 914, 455, 914 + 92, 455 + 78, "30") != null)
                        {
                            mouseclick(960, 651);
                            DD_key(602, 1); send_key(710); DD_key(602, 2); send_key(105); //새로고침
                            대기(딜레이(800)); 실패++; break;
                        }
                        if (실패 == 0) { DD_key(500, 1); send_key(300); DD_key(500, 2); send_key(603); break; }

                    } while (itemCoord == null);//구매하기()
                }
            }

        }
        void invoking(Control c, string data) //컨트롤은 모든 컨트롤클래스에서 상속해서 레이블을 만들고 컨트롤 클래스에서 상속해서, 즉 버튼은 버튼이 되고, 레이블은 레이블이 됨 모든 UI의 부모.
        {
            if (c.InvokeRequired)
                c.Invoke(new MethodInvoker(() =>
                {              // invoke는 UI스레드가 아닌, 작업스레드에서 UI권한을 받아올때 사용.
                    c.Text = data;
                }));
            else
                c.Text = data;

        }
        void invoking(PictureBox p, Bitmap bitmap) //컨트롤은 모든 컨트롤클래스에서 상속해서 레이블을 만들고 컨트롤 클래스에서 상속해서, 즉 버튼은 버튼이 되고, 레이블은 레이블이 됨 모든 UI의 부모.
        {
            if (p.InvokeRequired)
                p.Invoke(new MethodInvoker(() =>
                {              // invoke는 UI스레드가 아닌, 작업스레드에서 UI권한을 받아올때 사용.
                    p.Image = bitmap;
                }));
            else
                p.Image = bitmap;
        }
        void invoking(Button b) //컨트롤은 모든 컨트롤클래스에서 상속해서 레이블을 만들고 컨트롤 클래스에서 상속해서, 즉 버튼은 버튼이 되고, 레이블은 레이블이 됨 모든 UI의 부모.
        {
            if (b.InvokeRequired)
                b.Invoke(new MethodInvoker(() =>
                {              // invoke는 UI스레드가 아닌, 작업스레드에서 UI권한을 받아올때 사용.
                    b.PerformClick();
                }));
            else
                b.PerformClick();
                
        }
        void invoke_enable(Button b)
        {
            if (b.InvokeRequired)
                b.Invoke(new MethodInvoker(() =>
                {              // invoke는 UI스레드가 아닌, 작업스레드에서 UI권한을 받아올때 사용.
                    b.Enabled = true;
                }));
            else
                b.Enabled = true;
        }
        void invoking(CheckBox box, bool b)
        {
            if (box.InvokeRequired)
                box.Invoke(new MethodInvoker(() =>
                {              // invoke는 UI스레드가 아닌, 작업스레드에서 UI권한을 받아올때 사용.
                    box.Checked = b;
                }));
            else
                box.Checked = b;
        }
        void invoking(CheckBox box, CheckState state)
        {
            if (box.InvokeRequired)
                box.Invoke(new MethodInvoker(() =>
                {              // invoke는 UI스레드가 아닌, 작업스레드에서 UI권한을 받아올때 사용.
                    box.CheckState = state;
                }));
            else
                box.CheckState = state;
        }

        void update()
        {
            invoking(에너지, energy.ToString());
            invoking(카드갯수, card.ToString());
            invoking(카드로테이션, received_total.ToString());
            invoking(라운드, round.ToString());
            invoking(총카드, card_total.ToString());

            List<Button> buttons = new List<Button> {
                button1,button2,button3,button4,button5,button6,button7,button8,button9,button10,button11,button12
            };
            foreach (var b in buttons)
            {
                invoking(b, 엑시정보[buttons.IndexOf(b)].in_deck.ToString()); // indexof(b) 현재 b의 인덱스를 가져옴.
            }
            List<Tuple<Label, Label>> labels = new List<Tuple<Label, Label>> //tuple 은 햄버거 세트메뉴 만들어준거
            {
                new Tuple<Label,Label>(c11,c12), new Tuple<Label,Label>(c21,c22), new Tuple<Label,Label>(c31,c32),
                new Tuple<Label,Label>(c41,c42),new Tuple<Label,Label>(c51,c52),new Tuple<Label,Label>(c61,c62),
                new Tuple<Label,Label>(c71,c72),new Tuple<Label,Label>(c81,c82),new Tuple<Label,Label>(c91,c92),
                new Tuple<Label,Label>(c101,c102),new Tuple<Label,Label>(c111,c112),new Tuple<Label,Label>(c121,c122)
            };
            foreach (var l in labels)
            {
                invoking(l.Item1, 뽑기1개확률(엑시정보[labels.IndexOf(l)].in_deck, card_total, card, card_used)); // item1 => c11
                invoking(l.Item2, 뽑기2개확률(엑시정보[labels.IndexOf(l)].in_deck, card_total, card, card_used)); // item2 => c12
            }

            Queue<Bitmap> hand = new Queue<Bitmap> { };
            foreach (var a in 엑시정보)
            {
                for (int i = a.in_hand; i > 0; i--)
                    hand.Enqueue(a.사진);
            }
            List<PictureBox> pictureBoxes = new List<PictureBox>
            {
                pictureBox14,pictureBox15,pictureBox16,pictureBox17,pictureBox18,pictureBox19,pictureBox20,pictureBox21,pictureBox22,pictureBox23,pictureBox24,pictureBox25
            };
            foreach (var p in pictureBoxes)
            {

                Bitmap bit = null;  // 주의! exception error 발생할 수 있음.
                if (hand.Any()) // Any()는 bool 타입 리턴이며, 리스트 안에 데이터가 있으면 true 리턴.
                    bit = hand.Dequeue();
                invoking(p, bit);
            }
            /*            pictureBox14.Image = hand.FirstOrDefault(); if (hand.Count() > 0) hand.RemoveAt(0);
                        pictureBox15.Image = hand.FirstOrDefault(); if (hand.Count() > 0) hand.RemoveAt(0);
                        pictureBox16.Image = hand.FirstOrDefault(); if (hand.Count() > 0) hand.RemoveAt(0);
                        pictureBox17.Image = hand.FirstOrDefault(); if (hand.Count() > 0) hand.RemoveAt(0);
                        pictureBox18.Image = hand.FirstOrDefault(); if (hand.Count() > 0) hand.RemoveAt(0);
                        pictureBox19.Image = hand.FirstOrDefault(); if (hand.Count() > 0) hand.RemoveAt(0);
                        pictureBox20.Image = hand.FirstOrDefault(); if (hand.Count() > 0) hand.RemoveAt(0);
                        pictureBox21.Image = hand.FirstOrDefault(); if (hand.Count() > 0) hand.RemoveAt(0);
                        pictureBox22.Image = hand.FirstOrDefault(); if (hand.Count() > 0) hand.RemoveAt(0);
                        pictureBox23.Image = hand.FirstOrDefault(); if (hand.Count() > 0) hand.RemoveAt(0);
                        pictureBox24.Image = hand.FirstOrDefault(); if (hand.Count() > 0) hand.RemoveAt(0);
                        pictureBox25.Image = hand.FirstOrDefault(); if (hand.Count() > 0) hand.RemoveAt(0);*/

        }
        int card_to_int()
        {
            winactivate();
            Bitmap count_img = 스샷(1133, 93, 43, 26);
            //Clipboard.SetImage(count_img);
            if (imageMatching(count_img, ConvertToFormat(new Bitmap(경로변환("0개_1")))).Count > 0) return 0;
            if (imageMatching(count_img, ConvertToFormat(new Bitmap(경로변환("1개_1")))).Count > 0) return 1;
            if (imageMatching(count_img, ConvertToFormat(new Bitmap(경로변환("2개_1")))).Count > 0) return 2;
            if (imageMatching(count_img, ConvertToFormat(new Bitmap(경로변환("3개_1")))).Count > 0) return 3;
            if (imageMatching(count_img, ConvertToFormat(new Bitmap(경로변환("4개_1")))).Count > 0) return 4;
            if (imageMatching(count_img, ConvertToFormat(new Bitmap(경로변환("5개_1")))).Count > 0) return 5;
            if (imageMatching(count_img, ConvertToFormat(new Bitmap(경로변환("6개_1")))).Count > 0) return 6;
            if (imageMatching(count_img, ConvertToFormat(new Bitmap(경로변환("7개_1")))).Count > 0) return 7;
            if (imageMatching(count_img, ConvertToFormat(new Bitmap(경로변환("8개_1")))).Count > 0) return 8;
            if (imageMatching(count_img, ConvertToFormat(new Bitmap(경로변환("9개_1")))).Count > 0) return 9;
            if (imageMatching(count_img, ConvertToFormat(new Bitmap(경로변환("10개_1")))).Count > 0) return 10;
            if (imageMatching(count_img, ConvertToFormat(new Bitmap(경로변환("11개_1")))).Count > 0) return 11;
            if (imageMatching(count_img, ConvertToFormat(new Bitmap(경로변환("12개_1")))).Count > 0) return 12;

            return 12;
        }
        void died_check()
        {

            List<int> loca = new List<int> { };
            foreach (var location in 엑시정보.Where(a => a.died == false).Select(value => value)) // select(value => value)는 생략 가능 select는 그중에 하나만 가져올때 씀. 예) value.번호
                loca.Add(location.위치);
            loca = loca.Distinct().ToList(); // distinct() 중복제거.
            //mousemove(1210, 169); 대기(딜레이(300));
            foreach (var a in loca)
            {
                if (is_axie_died(a))
                {
                    Form2 popup = new Form2(/*a,UsedCard_in_round*/);
                    popup.Passposvalue = a;
                    popup.PassLastround_CardCnt = pre_round_card_cnt;
                    popup.Passgained_CardCnt = 0;
                    대기(딜레이(1000));
                    popup.ShowDialog();
                    if (popup.DialogResult == DialogResult.No)
                        return;
                    int lastround = popup.PassLastround_CardCnt;
                    int gained = popup.Passgained_CardCnt;
                    int enemy_loss = card + 3 - card_to_int(); //lastround - card_to_int() + gained;
                    for (int i = 0; i < enemy_loss; i++)
                        sub_card();

                    var 번호 = 엑시정보.Where(b => b.위치 == a).First().번호;
                    if (번호 < 4) // 0 1 2 3
                        invoking(button14);
                    //button14.PerformClick();
                    else if (번호 < 8) // 4 5 6 7 && 번호>=4 필요없음 (어차피 위에서 틀려서 내려옴)
                        invoking(button15); // 번호 누른것과 같은 효과 PerformClick()
                    else // 8 9 10 11
                        invoking(button16);

                }
            }

        }

        bool round_search(string name, 좌표 coord)
        {
            Bitmap screenshot = 스샷(coord.x, coord.y, coord.넓이, coord.높이);
            if (imageMatching(screenshot, ConvertToFormat(new Bitmap(경로변환(name)))).Count > 0)
                /*if (imageCoords(name, coord.x, coord.y, coord.넓이, coord.높이).Count > 0)*/
                return true;
            else return false;
        }
        bool endturn_search(string name, 좌표 coord)
        {
            Bitmap screenshot = 스샷(coord.x, coord.y, coord.넓이, coord.높이);
            if (imageMatching(screenshot, ConvertToFormat(new Bitmap(경로변환(name)))).Count > 0 || imageMatching(screenshot, ConvertToFormat(new Bitmap(경로변환("endturn2")))).Count > 0) return true;
            else return false;
        }
        Task round_check()
        {

            좌표 endturn자리 = new 좌표() { x = 1094, y = 501, 넓이 = 179, 높이 = 75 };
            좌표 round자리 = new 좌표() { x = 300, y = 95, 넓이 = 100, 높이 = 22 };
            if (!endturn_search("endturn", endturn자리) && !is_pre_round_card_counted)
            {
                pre_round_card_cnt = card;
                is_pre_round_card_counted = true;
            }
            if (round_search("round1", round자리) && endturn_search("endturn", endturn자리) && !newgame_checked)
            {
                is_roundcheck_running = false; // 무한루프 off
                return Task.CompletedTask;
            }


            switch (round)
            {
                case 1: if (round_search("round2", round자리) && endturn_search("endturn", endturn자리)) { 턴넘김(); newgame_checked = false; } break;
                case 2: if (round_search("round3", round자리) && endturn_search("endturn", endturn자리)) { 턴넘김(); newgame_checked = false; } break;
                case 3: if (round_search("round4", round자리) && endturn_search("endturn", endturn자리)) { 턴넘김(); newgame_checked = false; } break;
                case 4: if (round_search("round5", round자리) && endturn_search("endturn", endturn자리)) { 턴넘김(); newgame_checked = false; } break;
                case 5: if (round_search("round6", round자리) && endturn_search("endturn", endturn자리)) { 턴넘김(); newgame_checked = false; } break;
                case 6: if (round_search("round7", round자리) && endturn_search("endturn", endturn자리)) { 턴넘김(); newgame_checked = false; } break;
                case 7: if (round_search("round8", round자리) && endturn_search("endturn", endturn자리)) { 턴넘김(); newgame_checked = false; } break;
                case 8: if (round_search("round9", round자리) && endturn_search("endturn", endturn자리)) { 턴넘김(); newgame_checked = false; } break;
                case 9: if (round_search("round10", round자리) && endturn_search("endturn", endturn자리)) { 턴넘김(); newgame_checked = false; } break;
                case 10: if (round_search("round11", round자리) && endturn_search("endturn", endturn자리)) { 턴넘김(); newgame_checked = false; } break;
                case 11: if (round_search("round12", round자리) && endturn_search("endturn", endturn자리)) { 턴넘김(); newgame_checked = false; } break;
                case 12: if (round_search("round13", round자리) && endturn_search("endturn", endturn자리)) { 턴넘김(); newgame_checked = false; } break;
                case 13: if (round_search("round14", round자리) && endturn_search("endturn", endturn자리)) { 턴넘김(); newgame_checked = false; } break;
                case 14: if (round_search("round15", round자리) && endturn_search("endturn", endturn자리)) { 턴넘김(); newgame_checked = false; } break;
            }
            대기(딜레이(1000));
            return Task.CompletedTask;
        }
        void 턴넘김()
        {
            //died_check(); //엑시 죽었는지 확인.

            add_card(); add_card(); add_card();
            energy = energy + 2;
            round++;
            update(); rotate(); 상대카드갯수스샷();
            toggle = true; UsedCard_in_round = 0;
            is_pre_round_card_counted = false;
        }
        void 상대카드갯수스샷()
        {
            //Bitmap 준비완료 = 스샷(1120, 522, 130, 35);
            Bitmap 준비완료 = 스샷(1094, 501, 179, 75);
            Bitmap 카드수 = new Bitmap(10, 10);
            //if (imageMatching(준비완료, ConvertToFormat(new Bitmap(경로변환("준비완료")))).Count > 0 && toggle == true)
            if (imageMatching(준비완료, ConvertToFormat(new Bitmap(경로변환("endturn")))).Count > 0 && toggle == true)
            {
                카드수 = 스샷(1119, 54, 70, 68);
                pictureBox13.Image = 카드수;
                toggle = false; // 배틀중에 카드갯수 이미지찍도록 할 때 사용하던 요소 / 한번 찍었으면, 턴넘어가질때까지 안찍히게 
            }

        }

        void 게임초기화()
        {
            is_roundcheck_running = false;

            엑시정보.Clear();
            for (int i = 1; i < 13; i++)
                엑시정보.Add(new 카드정보 { 번호 = i, in_deck = 2 });

            //턴넘김();
            pre_round_card_cnt = 0;
            card = 6;
            energy = 3;
            round = 1;
            received_total = 6;
            card_total = 24;
            card_used = 0;

            //foreach(var but in button)
            //  button[but] = 2;

            update();
            List<CheckBox> boxes = new List<CheckBox>{
                checkBox1,checkBox2,checkBox3,checkBox4,checkBox5,checkBox6,checkBox7,checkBox8,checkBox9,checkBox10,checkBox11,checkBox12
            };

            foreach (var cb in boxes)
            {
                invoking(cb, false);
            }
            /*checkBox1.Checked = false; checkBox2.Checked = false; checkBox3.Checked = false; checkBox4.Checked = false;
            checkBox5.Checked = false; checkBox6.Checked = false; checkBox7.Checked = false; checkBox8.Checked = false;
            checkBox9.Checked = false; checkBox10.Checked = false; checkBox11.Checked = false; checkBox12.Checked = false;
            */
            invoke_enable(button14); invoke_enable(button15); invoke_enable(button16);
            /*            button14.Enabled = true; button15.Enabled = true; button16.Enabled = true;*/
            button1.Text = 엑시정보[0].in_deck.ToString(); button2.Text = 엑시정보[1].in_deck.ToString(); button3.Text = 엑시정보[2].in_deck.ToString();
            button4.Text = 엑시정보[3].in_deck.ToString(); button5.Text = 엑시정보[4].in_deck.ToString(); button6.Text = 엑시정보[5].in_deck.ToString();
            button7.Text = 엑시정보[6].in_deck.ToString(); button8.Text = 엑시정보[7].in_deck.ToString(); button9.Text = 엑시정보[8].in_deck.ToString();
            button10.Text = 엑시정보[9].in_deck.ToString(); button11.Text = 엑시정보[10].in_deck.ToString(); button12.Text = 엑시정보[11].in_deck.ToString();

        }
        private async Task 낚아채기()
        {
            SoundPlayer player = new SoundPlayer("노크.wav");

            좌표 itemCoord = new 좌표(0, 0);
            Bitmap ID = new Bitmap(583, 33);
            Graphics graphics = Graphics.FromImage(ID);
            graphics.CopyFromScreen(400, 280, 0, 0, ID.Size);
            ID = ConvertToFormat(ID); // 스크린샷 찍고, 색변환
            Bitmap id1 = ConvertToFormat(crop(ID, 0, 0, 90, 33));
            Bitmap id2 = ConvertToFormat(crop(ID, 236, 0, 90, 33));
            Bitmap id3 = ConvertToFormat(crop(ID, 471, 0, 90, 33));
            //Clipboard.SetImage(ConvertToFormat(ID));
            //Bitmap b = CopyToBpp(price, 1);
            //Clipboard.SetImage(b);

            do     //아쿠아틱 옵션 팝업 안보이면 값 받을때까지 반복
            {
                refresh(); // 페이지 새로고침하고 이더리움 마크 보일때까지 4초마다 새로고침
                mre.WaitOne();
                Bitmap test = new Bitmap(1920, 1080);
                graphics = Graphics.FromImage(test);
                graphics.CopyFromScreen(0, 0, 0, 0, test.Size);

                Bitmap ID_new = new Bitmap(583, 33);
                graphics = Graphics.FromImage(ID_new);
                graphics.CopyFromScreen(400, 280, 0, 0, ID_new.Size);
                ID_new = ConvertToFormat(ID_new); // 스크린샷 찍고, 색변환

                Bitmap price = new Bitmap(588, 27);
                graphics = Graphics.FromImage(price);
                graphics.CopyFromScreen(444, 495, 0, 0, price.Size);
                price = ConvertToFormat(price); // 스크린샷 찍고, 색변환

                if (CompareBitmaps(ID, ID_new))
                    continue;
                else
                {
                    int breaker = 0; // 하나 클릭해서 먹으면 다음거 먹지 않게 처리하는 스위치
                    좌표 특가좌표 = new 좌표(444, 495);
                    Bitmap 특가아이디 = ConvertToFormat(crop(ID_new, 0, 0, 90, 33));
                    int 실패 = 0;
                    특가찾기(ref price, "00", ref breaker, ref 특가좌표, ref 특가아이디, ref ID_new, ref id1, ref id2, ref id3, ref 실패, ref itemCoord);
                    특가찾기(ref price, "0.00", ref breaker, ref 특가좌표, ref 특가아이디, ref ID_new, ref id1, ref id2, ref id3, ref 실패, ref itemCoord);
                    특가찾기(ref price, "0.01", ref breaker, ref 특가좌표, ref 특가아이디, ref ID_new, ref id1, ref id2, ref id3, ref 실패, ref itemCoord);
                    특가찾기(ref price, "0.01x", ref breaker, ref 특가좌표, ref 특가아이디, ref ID_new, ref id1, ref id2, ref id3, ref 실패, ref itemCoord);
                    특가찾기(ref price, "0.02", ref breaker, ref 특가좌표, ref 특가아이디, ref ID_new, ref id1, ref id2, ref id3, ref 실패, ref itemCoord);

                    ID = ID_new;  // 아이디 새로고침 이후의 이미지로 최신화
                }
            } while (true);
        }
        public bool 로딩기다리기()
        {
            DateTime 시작시간 = DateTime.Now;// 현재시간 찍기.
            DateTime 총시작시간 = DateTime.Now;// 현재시간 찍기.
            int 분 = 60;
            좌표 itemCoord, itemCoord1;
            do     //아쿠아틱 옵션 팝업 안보이면 값 받을때까지 반복
            {
                Bitmap up = new Bitmap(1433, 596);
                Graphics graphics = Graphics.FromImage(up);
                graphics.CopyFromScreen(371, 245, 0, 0, up.Size);
                //itemCoord = 화면전환대기(itemname, 348, 302, 348+65, 302+100);
                itemCoord = (UseImageSearch(경로변환("시작점1"), 371, 245, 371 + 1433, 245 + 596, "30"));
                itemCoord1 = (UseImageSearch(경로변환("시작점2"), 371, 245, 371 + 1433, 245 + 596, "30"));
                TimeSpan 경과시간 = DateTime.Now - 시작시간;


                Bitmap cantfind = new Bitmap(43, 44); graphics = Graphics.FromImage(cantfind); graphics.CopyFromScreen(1034, 340, 0, 0, cantfind.Size); cantfind = ConvertToFormat(cantfind);
                if (imageMatching(cantfind, ConvertToFormat(new Bitmap(경로변환("못찾음")))).Count > 0)
                    return false;
                if (경과시간.TotalSeconds > 8)
                {
                    refresh();
                    클래스고르기(pre_Class);
                    브리딩카운트(pre_breed_min, pre_breed_max);
                    퓨어니스(pre_pure);
                    mouseclick(142, 259); 대기(딜레이(100));// 파츠탭 클릭;
                    foreach (var p in pre_parts)
                        파츠검색(p); mouseclick(330, 330);
                    시작시간 = DateTime.Now;
                }
                경과시간 = DateTime.Now - 총시작시간;
                if (경과시간.TotalSeconds > 분)
                {
                    // 카톡메시지 보내기
                    분 = 분 + 60;
                }
                mre.WaitOne();
            } while (itemCoord == null && itemCoord1 == null);
            대기(딜레이(2300));
            return true;

        }
        void refresh()
        {
            mouseclick(225, 207); // clear filter 
            대기(딜레이(700));
            int 분 = 60;
            좌표 itemCoord, itemCoord1;
            send_key(105); //새로고침
            대기(딜레이(3000));
            DateTime 시작시간 = DateTime.Now;// 현재시간 찍기.
            DateTime 총시작시간 = DateTime.Now;// 현재시간 찍기.
            mre.WaitOne();
            do     //아쿠아틱 옵션 팝업 안보이면 값 받을때까지 반복
            {

                //itemCoord = 화면전환대기(itemname, 348, 302, 348+65, 302+100);
                itemCoord = (UseImageSearch(경로변환("이더마크"), 447, 497, 447 + 18, 497 + 23, "30"));
                itemCoord1 = (UseImageSearch(경로변환("이더마크1"), 447, 497, 447 + 18, 497 + 23, "30"));
                TimeSpan 경과시간 = DateTime.Now - 시작시간;
                if (경과시간.TotalSeconds > 8)
                {
                    시작시간 = DateTime.Now;
                    send_key(105); //새로고침
                }
                경과시간 = DateTime.Now - 총시작시간;
                if (경과시간.TotalSeconds > 분)
                {
                    // 카톡메시지 보내기
                    분 = 분 + 60;
                }
                mre.WaitOne();
            } while (itemCoord == null && itemCoord1 == null);
            대기(딜레이(300));

        }
        void 브리딩카운트(int min, int max)
        {
            int x = 0;
            mouseclick(30, 580); 대기(딜레이(키입력지연));
            mouseclick(247, 580); 대기(딜레이(키입력지연));

            switch (min)
            {
                case 0: x = 30; break;
                case 1: x = 60; break;
                case 2: x = 90; break;
                case 3: x = 120; break;
                case 4: x = 150; break;
                case 5: x = 180; break;
                case 6: x = 210; break;
                case 7: x = 240; break;
            }
            mousedrag(30, 580, x, 580);
            switch (max)
            {
                case 0: x = 30; break;
                case 1: x = 60; break;
                case 2: x = 90; break;
                case 3: x = 120; break;
                case 4: x = 150; break;
                case 5: x = 180; break;
                case 6: x = 210; break;
                case 7: x = 240; break;
            }
            mousedrag(247, 580, x, 580);


        }
        void 퓨어니스(int x)
        {
            switch (x)
            {
                case 0: mouseclick(25, 750); break;
                case 1: mouseclick(60, 750); break;
                case 2: mouseclick(100, 750); break;
                case 3: mouseclick(135, 750); break;
                case 4: mouseclick(175, 750); break;
                case 5: mouseclick(215, 750); break;
                case 6: mouseclick(250, 750); break;
            }
        }
        void 클래스고르기(int 클래스)
        {

            switch (클래스)
            {
                case 1: mouseclick(27, 320); break;
                case 2: mouseclick(151, 320); break;
                case 3: mouseclick(27, 350); break;
                case 4: mouseclick(151, 350); break;
                case 5: mouseclick(27, 380); break;
                case 6: mouseclick(151, 380); break;
                case 7: mouseclick(27, 410); break;
                case 8: mouseclick(151, 410); break;
                case 9: mouseclick(27, 440); break;
            }
        }
        void 모두지움()
        {
            DD_key(600, 1); 대기(딜레이(키입력지연));
            send_key(401);
            DD_key(600, 2); 대기(딜레이(키입력지연));
            send_key(706);
        }

        void 파츠검색(string part)
        {

            mouseclick(117, 315); 대기(딜레이(100)); // 파츠검색란 클릭
            if (한영키변경완료 == false) 대기(딜레이(200));
            if (UseImageSearch(경로변환("한글"), 1758, 1043, 1758 + 33, 1043 + 34, "30") != null)
            {
                한영키변경(); 한영키변경완료 = true;
            }


            Send(파츠(part));
            if (part == "허밋")
            {
                대기(딜레이(100));
                send_아래();
            }

            send_enter();
            모두지움();

        }
        public List<string> 파츠초기화(List<string> parts)
        {
            List<string> del_parts = new List<string> { };
            mouseclick(330, 330); // 마우스좀 치우고
            for (int i = 5; i >= 0; i--) //맨 밑에서부터 // 한칸씩 내려가
            {
                int matched = 0; // 발견된것 0개로 초기화하고
                foreach (var p in parts)
                {
                    if (UseImageSearch(경로변환(p), 20, 342 + (i * 56), 20 + 223, 342 + ((i + 1) * 56), "30") != null) // 이전에 검색한 파츠리스트 중에 지금 검색하려는게 이미 올라와있으면
                    {
                        del_parts.Add(p); // 지울 목록에 추가(이미 있어서 검색대상에서 뺌)
                        matched++; // 검색된 사항 1개 추가.
                    }
                }
                if (matched == 0)  // 검색된 사항이 없으면
                {
                    좌표 x좌표 = UseImageSearch(경로변환("특성지움"), 20, 342 + (i * 56), 20 + 223, 342 + ((i + 1) * 56), "30"); // 그 자리에 있는 x표시 찾아서 있으면
                    if (x좌표 != null)
                    {
                        mouseclick(x좌표.x + (x좌표.넓이 / 2), x좌표.y + x좌표.높이 / 2); 대기(딜레이(500)); // 파츠 x클릭
                    }
                }

            }
            foreach (var dp in del_parts)
                parts.Remove(dp);

            //좌표 x좌표 = UseImageSearch(경로변환("x"), 172, 330, 172 + 110, 330 + 350, "30");

            return parts;
        }
        List<Bitmap> 엑시출석()
        {
            List<Bitmap> 출석부 = new List<Bitmap>();

            if (최신사진첩.Count() > 0)
            {
                foreach (var a in 최신사진첩)
                    이전사진첩.Add(a);
                최신사진첩.Clear();
            }
            if (로딩기다리기() == false)
                return 출석부;
            Bitmap 위 = new Bitmap(1433, 596);
            Graphics graphics = Graphics.FromImage(위);
            graphics.CopyFromScreen(371, 245, 0, 0, 위.Size);
            위 = ConvertToFormat(위);
            List<좌표> 위매칭 = imageMatching(위, ConvertToFormat(new Bitmap(경로변환("시작점1"))));
            if (위매칭.Count() == 0)
                위매칭 = imageMatching(위, ConvertToFormat(new Bitmap(경로변환("시작점2"))));
            foreach (var m in 위매칭)
            {

                출석부.Add(ConvertToFormat(crop(위, m.x - 20, m.y - 33, 71, 22)));
                최신사진첩.Add(ConvertToFormat(crop(위, m.x - 34, m.y - 52, 223, 272)));
            }


            mouseclick(1000, 229);
            send_key(708);
            대기(딜레이(300));

            Bitmap 아래 = new Bitmap(1444, 579);
            graphics = Graphics.FromImage(아래);
            graphics.CopyFromScreen(369, 361, 0, 0, 아래.Size);
            아래 = ConvertToFormat(아래);
            List<좌표> 아래매칭 = imageMatching(아래, ConvertToFormat(new Bitmap(경로변환("시작점1"))));
            if (아래매칭.Count() == 0)
                아래매칭 = imageMatching(아래, ConvertToFormat(new Bitmap(경로변환("시작점2"))));
            foreach (var m in 아래매칭)
            {
                int match_count = 0;
                foreach (var 출 in 출석부)
                {
                    if (CompareBitmaps(출, ConvertToFormat(crop(아래, m.x - 20, m.y - 33, 71, 22))))
                        match_count++;
                }

                if (match_count == 0)
                {
                    출석부.Add(ConvertToFormat(crop(아래, m.x - 20, m.y - 33, 71, 22)));
                    최신사진첩.Add(ConvertToFormat(crop(아래, m.x - 34, m.y - 52, 223, 272)));
                }

            }

            /*
                        List<좌표> 매칭 = new List<좌표>();
                        매칭.AddRange(위매칭);
                        매칭.AddRange(아래매칭);
            */
            return 출석부;
        }
        void 매물검사()
        {
            Bitmap 첫줄 = new Bitmap(1439, 290);
            Graphics graphics = Graphics.FromImage(첫줄);
            graphics.CopyFromScreen(370, 255, 0, 0, 첫줄.Size);
            첫줄 = ConvertToFormat(첫줄); // 스크린샷 찍고, 색변환

            for (int i = 0; i < 6; i++)
            {
                if (imageMatching(첫줄, ConvertToFormat(new Bitmap(경로변환("시작점")))).Count() > 0)
                {

                }

            }
        }
        void 엑시등록알림(int Class, int breed_min, int breed_max, int pure, List<string> parts)
        {
            string collection = "";
            foreach (var p in parts)
                collection += p;
            mre.WaitOne();
            if (pre_Class != Class || pre_breed_max != breed_max || pre_breed_min != breed_min || pre_pure != pure)     // 클래스, 브리딩카운트 퓨어니스 설정
            {
                mre.WaitOne();
                pre_Class = Class; pre_breed_min = breed_min; pre_breed_max = breed_max; pre_pure = pure; pre_parts = parts;
                refresh();
                클래스고르기(Class); 대기(딜레이(50));
                브리딩카운트(breed_min, breed_max); 대기(딜레이(50));
                퓨어니스(pure); 대기(딜레이(50));
                mouseclick(142, 259); 대기(딜레이(100));// 파츠탭 클릭;
            }
            else
            {
                pre_parts = parts;
                mouseclick(142, 259); 대기(딜레이(100));// 파츠탭 클릭;
                do
                {
                    좌표 x좌표 = UseImageSearch(경로변환("특성지움"), 202, 345, 275, 641, "30"); // 그 자리에 있는 x표시 찾아서 있으면
                    if (x좌표 != null)
                    {
                        mouseclick(x좌표.x + (x좌표.넓이 / 2), x좌표.y + x좌표.높이 / 2); 대기(딜레이(500)); // 파츠 x클릭
                    }
                    else
                        break;

                } while (true);

                //parts = 파츠초기화(parts);
            }
            /// 파츠 설정
            foreach (var p in parts)
                파츠검색(p); mouseclick(330, 330); // 마우스좀 치우고
            /// 이미지검색
            엑시업데이트(collection);



        }

        void 엑시업데이트(string collection)
        {
            List<Bitmap> 최신출석부 = new List<Bitmap>();
            List<Bitmap> 임시출석부 = new List<Bitmap>();
            mre.WaitOne();
            //test1.Add(new 파츠출석부 { parts = collection, name = 최신출석부, 사진 = 최신사진첩 });

            if (test1.Where(a => a.parts == collection).Count() < 1)
            {
                test1.Add(new 파츠출석부 { parts = collection, name = 엑시출석(), 사진 = 최신사진첩 });
            }
            else
            {
                임시출석부 = test1.SingleOrDefault(a => a.parts == collection).name;
                최신출석부 = 엑시출석();

                int counter = 0;
                foreach (var 최신 in 최신출석부)
                {
                    int i = 1;
                    foreach (var 이전 in 임시출석부)
                    {
                        if (CompareBitmaps(최신, 이전))
                        {
                            //Clipboard.SetImage(최신);
                            //Clipboard.SetImage(이전);
                            break;
                        }

                        if (임시출석부.Count() == i) // 모두 검사했는데 없으면 복사 클립보드
                        {                                               // 카카오톡 전송
                            Clipboard.SetDataObject(최신사진첩[counter]); mouseclick(1869, 973);
                            DD_key(600, 1); Send("v"); DD_key(600, 2); send_enter();
                            Clipboard.SetText(collection);
                            DD_key(600, 1); Send("v"); DD_key(600, 2); send_enter();

                        }

                        i++;

                    }

                    counter++;
                }
                test1.SingleOrDefault(a => a.parts == collection).name = 최신출석부;
                test1.SingleOrDefault(a => a.parts == collection).사진 = 최신사진첩;


            }



            /* /// 이미지검색
             if (이전출석부.Count() < 1) 
             {
                 이전출석부 = 엑시출석();
             }
             else
             {
                 최신출석부 = 엑시출석();
                 임시출석부 = 이전출석부;
                 int counter = 0;
                 foreach (var 최신 in 최신출석부)
                 {
                     int i = 1;
                     foreach (var 이전 in 이전출석부)
                     {
                         if (CompareBitmaps(최신, 이전))
                         {
                             Clipboard.SetImage(최신);
                             Clipboard.SetImage(이전);
                             break;
                         }

                         if (이전출석부.Count() == i)
                             Clipboard.SetImage(최신사진첩[counter]);
                         i++;

                     }

                     counter++;



                 }
             }*/


        }
        void 퀘스트완료()
        {
            mouseclick(rand(18, 58, 45, 49)); 대기(딜레이(5000));
            mouseclick(rand(489, 60, 58, 64)); 대기(딜레이(5000));
            mouseclick(rand(480, 417, 120, 38)); 대기(딜레이(5000));
            mouseclick(rand(1010, 422, 77, 29)); 대기(딜레이(5000));
            mouseclick(rand(1003, 263, 90, 31)); 대기(딜레이(5000));
        }

        void 어드벤처()
        {
            int count = 0;
            winactivate();
            while (true)
            {

                int w = 50;
                int h = 107;
                int x = 125;
                int y = 594;
                Bitmap endturn = new Bitmap(179, 75);
                Graphics graphics = Graphics.FromImage(endturn);
                graphics.CopyFromScreen(1094, 501, 0, 0, endturn.Size);
                endturn = ConvertToFormat(endturn); // 스크린샷 찍고, 색변환   

                Bitmap vic = new Bitmap(705, 156);
                Graphics graphics1 = Graphics.FromImage(vic);
                graphics1.CopyFromScreen(291, 314, 0, 0, vic.Size);
                vic = ConvertToFormat(vic); // 스크린샷 찍고, 색변환    

                Bitmap def = new Bitmap(809, 163);
                Graphics graphics2 = Graphics.FromImage(def);
                graphics2.CopyFromScreen(233, 311, 0, 0, def.Size);
                def = ConvertToFormat(def); // 스크린샷 찍고, 색변환    

                if (imageMatching(endturn, ConvertToFormat(new Bitmap(경로변환("endturn")))).Count > 0)
                {
                    for (int i = 0; i < 21; i++)
                    {
                        mouseclick(rand(x + (i * w), y, w, h));
                        대기(딜레이(300));
                    }
                    mouseclick(rand(1111, 515, 145, 49));
                    대기(딜레이(500));
                    mouseclick(rand(111, 515, 145, 49));
                    대기(딜레이(500));
                }

                if (imageMatching(vic, ConvertToFormat(new Bitmap(경로변환("victory")))).Count > 0 || imageMatching(def, ConvertToFormat(new Bitmap(경로변환("defeated")))).Count > 0)
                {
                    mouseclick(rand(1111, 515, 145, 49)); 대기(딜레이(1000));
                    mouseclick(rand(1111, 515, 145, 49)); 대기(딜레이(1000));
                    mouseclick(rand(1111, 515, 145, 49)); 대기(딜레이(7000));

                    if (count > 30)
                        break;
                    count++;
                    mouseclick(rand(784, 596, 427, 120)); 대기(딜레이(5000));
                }
            }
            퀘스트완료();

        }

        void axie_alert()
        {
            Bitmap 발견 = new Bitmap(10, 10);
            Bitmap screenshot = 스샷(548, 205, 87, 21);
            Bitmap 엑시슬롯1 = 스샷(457, 279, 110, 88);
            Bitmap connected = ConvertToFormat(new Bitmap(경로변환("connected")));
            Bitmap empty = ConvertToFormat(new Bitmap(경로변환("empty")));
            while (true)
            {
                screenshot = 스샷(548, 205, 87, 21);
                if (imageMatching(screenshot, connected).Count > 0)
                {
                    엑시슬롯1 = 스샷(457, 279, 110, 88);
                    if (imageMatching(엑시슬롯1, empty).Count > 0)
                    {
                        대기(딜레이(3000));
                        continue;
                    }
                    else
                    {
                        발견 = 스샷(417, 244, 1051, 205);
                        Clipboard.SetImage(발견);
                        mouseclick(1581, 763); 대기(딜레이(1000));
                        붙여넣기(); 대기(딜레이(500));
                        send_enter(); 대기(딜레이(500));
                        mouseclick(86, 52); 대기(딜레이(15000));
                        continue;
                    }

                }
                else
                {
                    mouseclick(86, 52); 대기(딜레이(15000)); continue;
                }
            }
        }

        public long GetC(int totalCount, int extractionCount) { return GetP(totalCount, extractionCount) / GetFactorial(extractionCount); }
        private long GetP(int totalCount, int extractionCount) { return GetFactorialDivision(totalCount, totalCount - extractionCount); }
        private long GetFactorialDivision(int topFactorial, int divisorFactorial) { long result = 1; for (int i = topFactorial; i > divisorFactorial; i--) { result *= i; } return result; }
        private long GetFactorial(int value) { if (value <= 1) { return 1; } return value * GetFactorial(value - 1); }


        string 뽑기1개확률(int 뽑을카드잔여갯수, int 총카드수, int 상대보유카드수, int 상대사용한카드수)
        {
            int temp = 0; int 미확인카드수 = 총카드수 - 상대사용한카드수;
            long 경우의수 = GetC(상대보유카드수, 1);
            double 원하는카드나올확률 = (double)뽑을카드잔여갯수 / 미확인카드수;
            미확인카드수--; 뽑을카드잔여갯수--;
            double 원하는카드안나올확률 = 1;
            temp = 미확인카드수;
            for (int i = 0; i < 상대보유카드수 - 1; i++)
            {
                원하는카드안나올확률 *= (double)(temp - 뽑을카드잔여갯수) / temp;
                temp--;
            }
            double sum = 경우의수 * 원하는카드나올확률 * 원하는카드안나올확률 * 100;
            string result = string.Format("{0:N2}", sum) + "%";


            /*double calculate = (double)GetC(상대보유카드수, 1) * (double)GetP(뽑을카드잔여갯수, 1) * (double)GetP(미확인카드수 - 뽑을카드잔여갯수, 상대보유카드수 - 1) / GetP(미확인카드수, 상대보유카드수) * 100;
            string result = string.Format("{0:N2}", calculate)+"%";*/
            return result;
        }
        string 뽑기2개확률(int 뽑을카드잔여갯수, int 총카드수, int 상대보유카드수, int 상대사용한카드수)
        {
            /*int 미확인카드수 = 총카드수 - 상대사용한카드수;
            double calculate = (double)GetC(상대보유카드수, 2) * (double)GetP(뽑을카드잔여갯수, 2) * (double)GetP(미확인카드수 - 뽑을카드잔여갯수, 상대보유카드수 - 2) / GetP(미확인카드수, 상대보유카드수) * 100;
            if (calculate < 0)
                calculate = 0;
            string result = string.Format("{0:N2}", calculate) + "%";*/
            int temp = 0; int 미확인카드수 = 총카드수 - 상대사용한카드수;
            long 경우의수 = GetC(상대보유카드수, 2);
            double 원하는카드나올확률 = (double)뽑을카드잔여갯수 / 미확인카드수 * (double)(뽑을카드잔여갯수 - 1) / (미확인카드수 - 1);
            미확인카드수--; 미확인카드수--; 뽑을카드잔여갯수--; 뽑을카드잔여갯수--;
            double 원하는카드안나올확률 = 1;
            temp = 미확인카드수;
            for (int i = 0; i < 상대보유카드수 - 2; i++)
            {
                원하는카드안나올확률 *= (double)(temp - 뽑을카드잔여갯수) / temp;
                temp--;
            }
            double sum = 경우의수 * 원하는카드나올확률 * 원하는카드안나올확률 * 100;
            string result = string.Format("{0:N2}", sum) + "%";

            return result;
        }
        void rotate()
        {
            if (received_total == card_total)
            {
                received_total = card;
                card_used = 0;
                foreach (var a in 엑시정보.Select((value, index) => (value, index)))
                {
                    var value = a.value;
                    var index = a.index;
                    if (a.value.died == false)
                    {
                        a.value.in_hand = a.value.in_deck + a.value.in_hand;
                        a.value.in_deck = 2 - a.value.in_hand;
                    }
                }

            }

            if (received_total < 0)
                received_total = card_total - 1;
        }
        void add_card()
        {
            card++; received_total++;
            rotate();
        }
        void sub_card()
        {
            card--; received_total--;
            rotate();
        }
        void loss_card()
        {
            card--;
            rotate(); update();
        }
        void undo_loss()
        {
            card++;
            rotate(); update();
        }
        좌표 get_curser()
        {
            int x = Cursor.Position.X;
            int y = Cursor.Position.Y;
            return new 좌표(x, y);
        }
        좌표 get_drag(좌표 p)
        {
            return new 좌표(p.x + 42, p.y + 33);
        }
        void 새게임()
        {
            while (is_roundcheck_running) //round1이 발견되지 않으면 여기 대기하고있다가 발견되자마자 새게임 시작
                대기(딜레이(1000));
            if (restart_pressed)
                return;
            winactivate();
            카드읽기();
            is_roundcheck_running = true;
            Task.Run(() =>
            {
                while (is_roundcheck_running) // 위 cts.Cancel()로 인해 이게 true로 바뀌니까 무한루프가 끝남. 
                {
                    round_check();
                }
            });
            Task.Run(() =>
            {
                새게임();
            });
        }
        void 확률()
        {
            int 남은카드수 = 0; int 시작카드수 = 6;
            int 받은카드 = 3; int 총카드수 = 24;
            int[] 엑시1 = new int[4] { 2, 2, 2, 2 };
            int[] 엑시2 = new int[4] { 2, 2, 2, 2 };
            int[] 엑시3 = new int[4] { 2, 2, 2, 2 };
            //상대가 엑시1_1카드를 1개 가지고 있을 확률 = 6C1*엑시[1]*(총카드수- 액시[1]
            //double 엑시1_1_1개확률 = (double)GetC(받은카드, 1) * (double)GetP(엑시1[0], 1) * (double)GetP(총카드수 - 엑시1[0], 받은카드 - 1) / (double)GetP(총카드수, 받은카드) * 100;
            string 출력 = 뽑기1개확률(엑시1[0], 총카드수, 받은카드, card_used);





            MessageBox.Show(출력);


            long 엑시1_1_2개확률 = GetC(받은카드, 2) * GetP(엑시1[0], 2) * GetP(총카드수 - 엑시1[0], 받은카드 - 1) / GetP(총카드수, 받은카드);




        }
        //[DllImport("kernel32.dll")]
        //internal static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, IntPtr dwProcessId);
        //[DllImport("kernel32.dll")]
        //internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, uint nSize, IntPtr lpNumberOfBytesRead);

        //[DllImport("user32.dll", SetLastError = true)]
        //static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        //[DllImport("user32.dll")]
        //static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        //internal static extern 
        //public void readprocess_method()
        //{
        //    IntPtr h_wnd = FindWindow(null, "MapleStory.exe");
        //    ulong process_id = 0;

        //    GetWindowThreadProcessId(h_wnd, &process_id);
        //    PROCESS_QUERY_INFORMATION = 0x0400;
        //    PROCESS_VM_READ = 0x0010;
        //    int base1 = 0x00007fffffffffff;
        //    IntPtr process = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, process_id);
        //    bool readprocess = ReadProcessMemory(process, );

        //    string nameprocess = "MapleStory.exe";

        //}

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x312)
            {
                var InputValue = (Keys)m.WParam;
                /*try
                {*/
                작업중인Task확인();
                switch (InputValue)
                {

                    case Keys.F2:
                        어드벤처();
                        //카드읽기(); //test
                        break;
                    case Keys.F3:
                        
                        axie_alert();
                        break;

                    case Keys.NumPad1:// 시작
                    case Keys.F1:// 시작
                        restart_pressed = true; // 이전에 돌고있던 작업스레드 모두 죽이기.
                        winactivate();
                        카드읽기();
                        is_roundcheck_running = true;
                        Task.Run(() =>
                        {
                            while (is_roundcheck_running) // 위 cts.Cancel()로 인해 이게 true로 바뀌니까 무한루프가 끝남. 
                            {
                                round_check();
                                if (restart_pressed)
                                    break;
                            }
                        });
                        Task.Run(() =>
                        {
                            새게임();
                        });
                        restart_pressed = false;

                        break;
                    case Keys.Subtract: // 에너지 감소 & 이미지 검색 멈춤/시작
                    case Keys.PageDown:
                        energy--;
                        에너지.Text = energy.ToString();
                        break;
                    case Keys.Add:// 에너지 증가
                    case Keys.PageUp:
                        energy++;
                        에너지.Text = energy.ToString();
                        if (false == (스위치 = !스위치))
                            mre.Reset();
                        else
                            mre.Set();
                        break;
                    case Keys.Divide:// 카드감소
                    case Keys.Delete:// 카드감소
                        sub_card();
                        update();
                        break;
                    case Keys.Multiply:// 카드증가
                    case Keys.Insert:// 카드증가
                        add_card();
                        update();
                        break;
                    case Keys.NumPad8:
                        loss_card();
                        break;
                    case Keys.NumPad9:
                        undo_loss();
                        break;
                    case Keys.NumPad0:// 턴넘김
                    case Keys.End:// 턴넘김
                        턴넘김();
                        break;
                    case Keys.F9:// 리셋
                        do
                        {
                            pre_Class = 0; pre_breed_max = 0; pre_breed_min = 0; pre_pure = 0;
                            //엑시등록알림(2, 0, 2, 5, new List<string> { "쿠쿠", "코이", "블루문" });
                            //엑시등록알림(2, 0, 2, 5, new List<string> { "쿠쿠", "코이", "람" });
                            ////엑시등록알림(2, 0, 2, 5, new List<string> { "아코", "니모", "리스키피쉬" });
                            ////엑시등록알림(2, 0, 2, 5, new List<string> { "리스키비스트", "니모", "리스키피쉬" });
                            ////엑시등록알림(2, 0, 7, 5, new List<string> { "쿠쿠", "코이", "블루문", "람" });
                            ////엑시등록알림(2, 0, 7, 5, new List<string> { "리틀브랜치", "니모", "허밋", "리스키피쉬"});
                            ////엑시등록알림(2, 0, 7, 5, new List<string> { "아코", "니모","리스키비스트", "리스키피쉬"});
                            //엑시등록알림(2, 0, 2, 0, new List<string> { "리틀브랜치", "듀블", "니모", "리스키피쉬"});
                            //엑시등록알림(9, 0, 2, 5, new List<string> { "스네일쉘", "니모", "안테나", "큐트버니" });
                            //엑시등록알림(6, 0, 2, 5, new List<string> { "인싸이저", "니모","가리시웜", "큐트버니" });
                            //엑시등록알림(2, 0, 2, 5, new List<string> { "리틀브랜치", "핫벗", "시리어스" });

                            대기(딜레이(5000));
                        } while (true);

                    case Keys.F10:// 게임시작
                        게임초기화();

                        break;


                    case Keys.F11:
                        Application.Exit();
                        break;


                }

            }
            /*catch (Exception)
            {
            }
        }*/

        }





        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        /// 
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.pictureBox6 = new System.Windows.Forms.PictureBox();
            this.pictureBox7 = new System.Windows.Forms.PictureBox();
            this.pictureBox8 = new System.Windows.Forms.PictureBox();
            this.pictureBox9 = new System.Windows.Forms.PictureBox();
            this.pictureBox10 = new System.Windows.Forms.PictureBox();
            this.pictureBox11 = new System.Windows.Forms.PictureBox();
            this.pictureBox12 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.에너지 = new System.Windows.Forms.Label();
            this.카드갯수 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.checkBox7 = new System.Windows.Forms.CheckBox();
            this.checkBox8 = new System.Windows.Forms.CheckBox();
            this.checkBox9 = new System.Windows.Forms.CheckBox();
            this.checkBox10 = new System.Windows.Forms.CheckBox();
            this.checkBox11 = new System.Windows.Forms.CheckBox();
            this.checkBox12 = new System.Windows.Forms.CheckBox();
            this.카드로테이션 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.라운드 = new System.Windows.Forms.Label();
            this.button14 = new System.Windows.Forms.Button();
            this.button15 = new System.Windows.Forms.Button();
            this.button16 = new System.Windows.Forms.Button();
            this.pictureBox13 = new System.Windows.Forms.PictureBox();
            this.c11 = new System.Windows.Forms.Label();
            this.c12 = new System.Windows.Forms.Label();
            this.c21 = new System.Windows.Forms.Label();
            this.c22 = new System.Windows.Forms.Label();
            this.c31 = new System.Windows.Forms.Label();
            this.c32 = new System.Windows.Forms.Label();
            this.c41 = new System.Windows.Forms.Label();
            this.c42 = new System.Windows.Forms.Label();
            this.c51 = new System.Windows.Forms.Label();
            this.c61 = new System.Windows.Forms.Label();
            this.c71 = new System.Windows.Forms.Label();
            this.c81 = new System.Windows.Forms.Label();
            this.c52 = new System.Windows.Forms.Label();
            this.c62 = new System.Windows.Forms.Label();
            this.c72 = new System.Windows.Forms.Label();
            this.c82 = new System.Windows.Forms.Label();
            this.c91 = new System.Windows.Forms.Label();
            this.c101 = new System.Windows.Forms.Label();
            this.c111 = new System.Windows.Forms.Label();
            this.c121 = new System.Windows.Forms.Label();
            this.c92 = new System.Windows.Forms.Label();
            this.c102 = new System.Windows.Forms.Label();
            this.c112 = new System.Windows.Forms.Label();
            this.c122 = new System.Windows.Forms.Label();
            this.pictureBox14 = new System.Windows.Forms.PictureBox();
            this.pictureBox15 = new System.Windows.Forms.PictureBox();
            this.pictureBox16 = new System.Windows.Forms.PictureBox();
            this.pictureBox17 = new System.Windows.Forms.PictureBox();
            this.pictureBox18 = new System.Windows.Forms.PictureBox();
            this.pictureBox19 = new System.Windows.Forms.PictureBox();
            this.pictureBox20 = new System.Windows.Forms.PictureBox();
            this.pictureBox21 = new System.Windows.Forms.PictureBox();
            this.pictureBox22 = new System.Windows.Forms.PictureBox();
            this.pictureBox23 = new System.Windows.Forms.PictureBox();
            this.pictureBox24 = new System.Windows.Forms.PictureBox();
            this.pictureBox25 = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.총카드 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox11)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox13)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox14)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox15)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox16)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox17)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox18)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox19)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox20)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox21)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox22)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox23)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox24)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox25)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(16, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(105, 146);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(127, 12);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(105, 146);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 0;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Location = new System.Drawing.Point(238, 12);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(105, 146);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 0;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox4
            // 
            this.pictureBox4.Location = new System.Drawing.Point(349, 12);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(105, 146);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox4.TabIndex = 0;
            this.pictureBox4.TabStop = false;
            this.pictureBox4.Click += new System.EventHandler(this.pictureBox4_Click);
            // 
            // pictureBox5
            // 
            this.pictureBox5.Location = new System.Drawing.Point(16, 215);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(105, 146);
            this.pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox5.TabIndex = 0;
            this.pictureBox5.TabStop = false;
            // 
            // pictureBox6
            // 
            this.pictureBox6.Location = new System.Drawing.Point(127, 215);
            this.pictureBox6.Name = "pictureBox6";
            this.pictureBox6.Size = new System.Drawing.Size(105, 146);
            this.pictureBox6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox6.TabIndex = 0;
            this.pictureBox6.TabStop = false;
            // 
            // pictureBox7
            // 
            this.pictureBox7.Location = new System.Drawing.Point(238, 217);
            this.pictureBox7.Name = "pictureBox7";
            this.pictureBox7.Size = new System.Drawing.Size(105, 144);
            this.pictureBox7.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox7.TabIndex = 0;
            this.pictureBox7.TabStop = false;
            // 
            // pictureBox8
            // 
            this.pictureBox8.Location = new System.Drawing.Point(349, 217);
            this.pictureBox8.Name = "pictureBox8";
            this.pictureBox8.Size = new System.Drawing.Size(105, 144);
            this.pictureBox8.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox8.TabIndex = 0;
            this.pictureBox8.TabStop = false;
            // 
            // pictureBox9
            // 
            this.pictureBox9.Location = new System.Drawing.Point(16, 423);
            this.pictureBox9.Name = "pictureBox9";
            this.pictureBox9.Size = new System.Drawing.Size(105, 146);
            this.pictureBox9.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox9.TabIndex = 0;
            this.pictureBox9.TabStop = false;
            // 
            // pictureBox10
            // 
            this.pictureBox10.Location = new System.Drawing.Point(128, 423);
            this.pictureBox10.Name = "pictureBox10";
            this.pictureBox10.Size = new System.Drawing.Size(105, 146);
            this.pictureBox10.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox10.TabIndex = 0;
            this.pictureBox10.TabStop = false;
            // 
            // pictureBox11
            // 
            this.pictureBox11.Location = new System.Drawing.Point(239, 423);
            this.pictureBox11.Name = "pictureBox11";
            this.pictureBox11.Size = new System.Drawing.Size(105, 146);
            this.pictureBox11.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox11.TabIndex = 0;
            this.pictureBox11.TabStop = false;
            // 
            // pictureBox12
            // 
            this.pictureBox12.Location = new System.Drawing.Point(350, 423);
            this.pictureBox12.Name = "pictureBox12";
            this.pictureBox12.Size = new System.Drawing.Size(105, 146);
            this.pictureBox12.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox12.TabIndex = 0;
            this.pictureBox12.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("굴림", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(460, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "상대 에너지";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("굴림", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(470, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "상대 카드";
            this.label2.Click += new System.EventHandler(this.label2_Click_1);
            // 
            // 에너지
            // 
            this.에너지.AutoSize = true;
            this.에너지.Font = new System.Drawing.Font("굴림", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.에너지.Location = new System.Drawing.Point(483, 41);
            this.에너지.Name = "에너지";
            this.에너지.Size = new System.Drawing.Size(0, 20);
            this.에너지.TabIndex = 2;
            // 
            // 카드갯수
            // 
            this.카드갯수.AutoSize = true;
            this.카드갯수.Font = new System.Drawing.Font("굴림", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.카드갯수.Location = new System.Drawing.Point(483, 138);
            this.카드갯수.Name = "카드갯수";
            this.카드갯수.Size = new System.Drawing.Size(0, 20);
            this.카드갯수.TabIndex = 2;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(107, 16);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(15, 14);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(218, 16);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(15, 14);
            this.checkBox2.TabIndex = 5;
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(329, 16);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(15, 14);
            this.checkBox3.TabIndex = 5;
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(440, 16);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(15, 14);
            this.checkBox4.TabIndex = 5;
            this.checkBox4.UseVisualStyleBackColor = true;
            this.checkBox4.CheckedChanged += new System.EventHandler(this.checkBox4_CheckedChanged);
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Location = new System.Drawing.Point(107, 215);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(15, 14);
            this.checkBox5.TabIndex = 5;
            this.checkBox5.UseVisualStyleBackColor = true;
            this.checkBox5.CheckedChanged += new System.EventHandler(this.checkBox5_CheckedChanged);
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.Location = new System.Drawing.Point(218, 215);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(15, 14);
            this.checkBox6.TabIndex = 5;
            this.checkBox6.UseVisualStyleBackColor = true;
            this.checkBox6.CheckedChanged += new System.EventHandler(this.checkBox6_CheckedChanged);
            // 
            // checkBox7
            // 
            this.checkBox7.AutoSize = true;
            this.checkBox7.Location = new System.Drawing.Point(329, 217);
            this.checkBox7.Name = "checkBox7";
            this.checkBox7.Size = new System.Drawing.Size(15, 14);
            this.checkBox7.TabIndex = 5;
            this.checkBox7.UseVisualStyleBackColor = true;
            this.checkBox7.CheckedChanged += new System.EventHandler(this.checkBox7_CheckedChanged);
            // 
            // checkBox8
            // 
            this.checkBox8.AutoSize = true;
            this.checkBox8.Location = new System.Drawing.Point(440, 215);
            this.checkBox8.Name = "checkBox8";
            this.checkBox8.Size = new System.Drawing.Size(15, 14);
            this.checkBox8.TabIndex = 5;
            this.checkBox8.UseVisualStyleBackColor = true;
            this.checkBox8.CheckedChanged += new System.EventHandler(this.checkBox8_CheckedChanged);
            // 
            // checkBox9
            // 
            this.checkBox9.AutoSize = true;
            this.checkBox9.Location = new System.Drawing.Point(107, 430);
            this.checkBox9.Name = "checkBox9";
            this.checkBox9.Size = new System.Drawing.Size(15, 14);
            this.checkBox9.TabIndex = 5;
            this.checkBox9.UseVisualStyleBackColor = true;
            this.checkBox9.CheckedChanged += new System.EventHandler(this.checkBox9_CheckedChanged);
            // 
            // checkBox10
            // 
            this.checkBox10.AutoSize = true;
            this.checkBox10.Location = new System.Drawing.Point(218, 430);
            this.checkBox10.Name = "checkBox10";
            this.checkBox10.Size = new System.Drawing.Size(15, 14);
            this.checkBox10.TabIndex = 5;
            this.checkBox10.UseVisualStyleBackColor = true;
            this.checkBox10.CheckedChanged += new System.EventHandler(this.checkBox10_CheckedChanged);
            // 
            // checkBox11
            // 
            this.checkBox11.AutoSize = true;
            this.checkBox11.Location = new System.Drawing.Point(329, 430);
            this.checkBox11.Name = "checkBox11";
            this.checkBox11.Size = new System.Drawing.Size(15, 14);
            this.checkBox11.TabIndex = 5;
            this.checkBox11.UseVisualStyleBackColor = true;
            this.checkBox11.CheckedChanged += new System.EventHandler(this.checkBox11_CheckedChanged);
            // 
            // checkBox12
            // 
            this.checkBox12.AutoSize = true;
            this.checkBox12.Location = new System.Drawing.Point(440, 430);
            this.checkBox12.Name = "checkBox12";
            this.checkBox12.Size = new System.Drawing.Size(15, 14);
            this.checkBox12.TabIndex = 5;
            this.checkBox12.UseVisualStyleBackColor = true;
            this.checkBox12.CheckedChanged += new System.EventHandler(this.checkBox12_CheckedChanged);
            // 
            // 카드로테이션
            // 
            this.카드로테이션.AutoSize = true;
            this.카드로테이션.Font = new System.Drawing.Font("굴림", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.카드로테이션.Location = new System.Drawing.Point(483, 217);
            this.카드로테이션.Name = "카드로테이션";
            this.카드로테이션.Size = new System.Drawing.Size(0, 20);
            this.카드로테이션.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("굴림", 10F);
            this.label6.Location = new System.Drawing.Point(458, 190);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(120, 14);
            this.label6.TabIndex = 3;
            this.label6.Text = "상대 총 받은 카드";
            this.label6.Click += new System.EventHandler(this.label2_Click_1);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(30, 186);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(141, 186);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 6;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(252, 186);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 6;
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(363, 186);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 6;
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.Button4_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(31, 604);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(75, 23);
            this.button9.TabIndex = 6;
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.Button9_Click);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(142, 604);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(75, 23);
            this.button10.TabIndex = 6;
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.Button10_Click);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(253, 604);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(75, 23);
            this.button11.TabIndex = 6;
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.Button11_Click);
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(364, 604);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(75, 23);
            this.button12.TabIndex = 6;
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.Button12_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(30, 394);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 6;
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.Button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(141, 394);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 6;
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.Button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(252, 394);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 23);
            this.button7.TabIndex = 6;
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.Button7_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(363, 394);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(75, 23);
            this.button8.TabIndex = 6;
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.Button8_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("굴림", 10F);
            this.label3.Location = new System.Drawing.Point(460, 293);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 14);
            this.label3.TabIndex = 3;
            this.label3.Text = "Round";
            this.label3.Click += new System.EventHandler(this.label2_Click_1);
            // 
            // 라운드
            // 
            this.라운드.AutoSize = true;
            this.라운드.Font = new System.Drawing.Font("굴림", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.라운드.Location = new System.Drawing.Point(477, 320);
            this.라운드.Name = "라운드";
            this.라운드.Size = new System.Drawing.Size(0, 20);
            this.라운드.TabIndex = 2;
            // 
            // button14
            // 
            this.button14.Location = new System.Drawing.Point(460, 72);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(38, 23);
            this.button14.TabIndex = 7;
            this.button14.Text = "Died";
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Click += new System.EventHandler(this.button14_Click);
            // 
            // button15
            // 
            this.button15.Location = new System.Drawing.Point(460, 267);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(38, 23);
            this.button15.TabIndex = 7;
            this.button15.Text = "Died";
            this.button15.UseVisualStyleBackColor = true;
            this.button15.Click += new System.EventHandler(this.button15_Click);
            // 
            // button16
            // 
            this.button16.Location = new System.Drawing.Point(460, 486);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(38, 23);
            this.button16.TabIndex = 7;
            this.button16.Text = "Died";
            this.button16.UseVisualStyleBackColor = true;
            this.button16.Click += new System.EventHandler(this.button16_Click);
            // 
            // pictureBox13
            // 
            this.pictureBox13.Location = new System.Drawing.Point(511, 127);
            this.pictureBox13.Name = "pictureBox13";
            this.pictureBox13.Size = new System.Drawing.Size(55, 57);
            this.pictureBox13.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox13.TabIndex = 8;
            this.pictureBox13.TabStop = false;
            // 
            // c11
            // 
            this.c11.AutoSize = true;
            this.c11.Location = new System.Drawing.Point(29, 171);
            this.c11.Name = "c11";
            this.c11.Size = new System.Drawing.Size(15, 12);
            this.c11.TabIndex = 9;
            this.c11.Text = "%";
            this.c11.Click += new System.EventHandler(this.label4_Click);
            // 
            // c12
            // 
            this.c12.AutoSize = true;
            this.c12.Location = new System.Drawing.Point(74, 171);
            this.c12.Name = "c12";
            this.c12.Size = new System.Drawing.Size(15, 12);
            this.c12.TabIndex = 9;
            this.c12.Text = "%";
            this.c12.Click += new System.EventHandler(this.label4_Click);
            // 
            // c21
            // 
            this.c21.AutoSize = true;
            this.c21.Location = new System.Drawing.Point(139, 171);
            this.c21.Name = "c21";
            this.c21.Size = new System.Drawing.Size(15, 12);
            this.c21.TabIndex = 9;
            this.c21.Text = "%";
            this.c21.Click += new System.EventHandler(this.label4_Click);
            // 
            // c22
            // 
            this.c22.AutoSize = true;
            this.c22.Location = new System.Drawing.Point(184, 171);
            this.c22.Name = "c22";
            this.c22.Size = new System.Drawing.Size(15, 12);
            this.c22.TabIndex = 9;
            this.c22.Text = "%";
            this.c22.Click += new System.EventHandler(this.label4_Click);
            // 
            // c31
            // 
            this.c31.AutoSize = true;
            this.c31.Location = new System.Drawing.Point(250, 171);
            this.c31.Name = "c31";
            this.c31.Size = new System.Drawing.Size(15, 12);
            this.c31.TabIndex = 9;
            this.c31.Text = "%";
            this.c31.Click += new System.EventHandler(this.label4_Click);
            // 
            // c32
            // 
            this.c32.AutoSize = true;
            this.c32.Location = new System.Drawing.Point(295, 171);
            this.c32.Name = "c32";
            this.c32.Size = new System.Drawing.Size(15, 12);
            this.c32.TabIndex = 9;
            this.c32.Text = "%";
            this.c32.Click += new System.EventHandler(this.label4_Click);
            // 
            // c41
            // 
            this.c41.AutoSize = true;
            this.c41.Location = new System.Drawing.Point(361, 171);
            this.c41.Name = "c41";
            this.c41.Size = new System.Drawing.Size(15, 12);
            this.c41.TabIndex = 9;
            this.c41.Text = "%";
            this.c41.Click += new System.EventHandler(this.label4_Click);
            // 
            // c42
            // 
            this.c42.AutoSize = true;
            this.c42.Location = new System.Drawing.Point(406, 171);
            this.c42.Name = "c42";
            this.c42.Size = new System.Drawing.Size(15, 12);
            this.c42.TabIndex = 9;
            this.c42.Text = "%";
            this.c42.Click += new System.EventHandler(this.label4_Click);
            // 
            // c51
            // 
            this.c51.AutoSize = true;
            this.c51.Location = new System.Drawing.Point(29, 379);
            this.c51.Name = "c51";
            this.c51.Size = new System.Drawing.Size(15, 12);
            this.c51.TabIndex = 9;
            this.c51.Text = "%";
            this.c51.Click += new System.EventHandler(this.label4_Click);
            // 
            // c61
            // 
            this.c61.AutoSize = true;
            this.c61.Location = new System.Drawing.Point(139, 379);
            this.c61.Name = "c61";
            this.c61.Size = new System.Drawing.Size(15, 12);
            this.c61.TabIndex = 9;
            this.c61.Text = "%";
            this.c61.Click += new System.EventHandler(this.label4_Click);
            // 
            // c71
            // 
            this.c71.AutoSize = true;
            this.c71.Location = new System.Drawing.Point(250, 379);
            this.c71.Name = "c71";
            this.c71.Size = new System.Drawing.Size(15, 12);
            this.c71.TabIndex = 9;
            this.c71.Text = "%";
            this.c71.Click += new System.EventHandler(this.label4_Click);
            // 
            // c81
            // 
            this.c81.AutoSize = true;
            this.c81.Location = new System.Drawing.Point(361, 379);
            this.c81.Name = "c81";
            this.c81.Size = new System.Drawing.Size(15, 12);
            this.c81.TabIndex = 9;
            this.c81.Text = "%";
            this.c81.Click += new System.EventHandler(this.label4_Click);
            // 
            // c52
            // 
            this.c52.AutoSize = true;
            this.c52.Location = new System.Drawing.Point(74, 379);
            this.c52.Name = "c52";
            this.c52.Size = new System.Drawing.Size(15, 12);
            this.c52.TabIndex = 9;
            this.c52.Text = "%";
            this.c52.Click += new System.EventHandler(this.label4_Click);
            // 
            // c62
            // 
            this.c62.AutoSize = true;
            this.c62.Location = new System.Drawing.Point(184, 379);
            this.c62.Name = "c62";
            this.c62.Size = new System.Drawing.Size(15, 12);
            this.c62.TabIndex = 9;
            this.c62.Text = "%";
            this.c62.Click += new System.EventHandler(this.label4_Click);
            // 
            // c72
            // 
            this.c72.AutoSize = true;
            this.c72.Location = new System.Drawing.Point(295, 379);
            this.c72.Name = "c72";
            this.c72.Size = new System.Drawing.Size(15, 12);
            this.c72.TabIndex = 9;
            this.c72.Text = "%";
            this.c72.Click += new System.EventHandler(this.label4_Click);
            // 
            // c82
            // 
            this.c82.AutoSize = true;
            this.c82.Location = new System.Drawing.Point(406, 379);
            this.c82.Name = "c82";
            this.c82.Size = new System.Drawing.Size(15, 12);
            this.c82.TabIndex = 9;
            this.c82.Text = "%";
            this.c82.Click += new System.EventHandler(this.label4_Click);
            // 
            // c91
            // 
            this.c91.AutoSize = true;
            this.c91.Location = new System.Drawing.Point(29, 589);
            this.c91.Name = "c91";
            this.c91.Size = new System.Drawing.Size(15, 12);
            this.c91.TabIndex = 9;
            this.c91.Text = "%";
            this.c91.Click += new System.EventHandler(this.label4_Click);
            // 
            // c101
            // 
            this.c101.AutoSize = true;
            this.c101.Location = new System.Drawing.Point(139, 589);
            this.c101.Name = "c101";
            this.c101.Size = new System.Drawing.Size(15, 12);
            this.c101.TabIndex = 9;
            this.c101.Text = "%";
            this.c101.Click += new System.EventHandler(this.label4_Click);
            // 
            // c111
            // 
            this.c111.AutoSize = true;
            this.c111.Location = new System.Drawing.Point(250, 589);
            this.c111.Name = "c111";
            this.c111.Size = new System.Drawing.Size(15, 12);
            this.c111.TabIndex = 9;
            this.c111.Text = "%";
            this.c111.Click += new System.EventHandler(this.label4_Click);
            // 
            // c121
            // 
            this.c121.AutoSize = true;
            this.c121.Location = new System.Drawing.Point(361, 589);
            this.c121.Name = "c121";
            this.c121.Size = new System.Drawing.Size(15, 12);
            this.c121.TabIndex = 9;
            this.c121.Text = "%";
            this.c121.Click += new System.EventHandler(this.label4_Click);
            // 
            // c92
            // 
            this.c92.AutoSize = true;
            this.c92.Location = new System.Drawing.Point(74, 589);
            this.c92.Name = "c92";
            this.c92.Size = new System.Drawing.Size(15, 12);
            this.c92.TabIndex = 9;
            this.c92.Text = "%";
            this.c92.Click += new System.EventHandler(this.label4_Click);
            // 
            // c102
            // 
            this.c102.AutoSize = true;
            this.c102.Location = new System.Drawing.Point(184, 589);
            this.c102.Name = "c102";
            this.c102.Size = new System.Drawing.Size(15, 12);
            this.c102.TabIndex = 9;
            this.c102.Text = "%";
            this.c102.Click += new System.EventHandler(this.label4_Click);
            // 
            // c112
            // 
            this.c112.AutoSize = true;
            this.c112.Location = new System.Drawing.Point(295, 589);
            this.c112.Name = "c112";
            this.c112.Size = new System.Drawing.Size(15, 12);
            this.c112.TabIndex = 9;
            this.c112.Text = "%";
            this.c112.Click += new System.EventHandler(this.label4_Click);
            // 
            // c122
            // 
            this.c122.AutoSize = true;
            this.c122.Location = new System.Drawing.Point(406, 589);
            this.c122.Name = "c122";
            this.c122.Size = new System.Drawing.Size(15, 12);
            this.c122.TabIndex = 9;
            this.c122.Text = "%";
            this.c122.Click += new System.EventHandler(this.label4_Click);
            // 
            // pictureBox14
            // 
            this.pictureBox14.Location = new System.Drawing.Point(16, 634);
            this.pictureBox14.Name = "pictureBox14";
            this.pictureBox14.Size = new System.Drawing.Size(73, 96);
            this.pictureBox14.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox14.TabIndex = 10;
            this.pictureBox14.TabStop = false;
            // 
            // pictureBox15
            // 
            this.pictureBox15.Location = new System.Drawing.Point(95, 634);
            this.pictureBox15.Name = "pictureBox15";
            this.pictureBox15.Size = new System.Drawing.Size(73, 96);
            this.pictureBox15.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox15.TabIndex = 10;
            this.pictureBox15.TabStop = false;
            // 
            // pictureBox16
            // 
            this.pictureBox16.Location = new System.Drawing.Point(174, 634);
            this.pictureBox16.Name = "pictureBox16";
            this.pictureBox16.Size = new System.Drawing.Size(73, 96);
            this.pictureBox16.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox16.TabIndex = 10;
            this.pictureBox16.TabStop = false;
            // 
            // pictureBox17
            // 
            this.pictureBox17.Location = new System.Drawing.Point(252, 634);
            this.pictureBox17.Name = "pictureBox17";
            this.pictureBox17.Size = new System.Drawing.Size(73, 96);
            this.pictureBox17.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox17.TabIndex = 10;
            this.pictureBox17.TabStop = false;
            // 
            // pictureBox18
            // 
            this.pictureBox18.Location = new System.Drawing.Point(331, 634);
            this.pictureBox18.Name = "pictureBox18";
            this.pictureBox18.Size = new System.Drawing.Size(73, 96);
            this.pictureBox18.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox18.TabIndex = 10;
            this.pictureBox18.TabStop = false;
            // 
            // pictureBox19
            // 
            this.pictureBox19.Location = new System.Drawing.Point(410, 634);
            this.pictureBox19.Name = "pictureBox19";
            this.pictureBox19.Size = new System.Drawing.Size(73, 96);
            this.pictureBox19.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox19.TabIndex = 10;
            this.pictureBox19.TabStop = false;
            // 
            // pictureBox20
            // 
            this.pictureBox20.Location = new System.Drawing.Point(16, 736);
            this.pictureBox20.Name = "pictureBox20";
            this.pictureBox20.Size = new System.Drawing.Size(73, 96);
            this.pictureBox20.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox20.TabIndex = 10;
            this.pictureBox20.TabStop = false;
            // 
            // pictureBox21
            // 
            this.pictureBox21.Location = new System.Drawing.Point(95, 736);
            this.pictureBox21.Name = "pictureBox21";
            this.pictureBox21.Size = new System.Drawing.Size(73, 96);
            this.pictureBox21.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox21.TabIndex = 10;
            this.pictureBox21.TabStop = false;
            // 
            // pictureBox22
            // 
            this.pictureBox22.Location = new System.Drawing.Point(174, 736);
            this.pictureBox22.Name = "pictureBox22";
            this.pictureBox22.Size = new System.Drawing.Size(73, 96);
            this.pictureBox22.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox22.TabIndex = 10;
            this.pictureBox22.TabStop = false;
            // 
            // pictureBox23
            // 
            this.pictureBox23.Location = new System.Drawing.Point(252, 736);
            this.pictureBox23.Name = "pictureBox23";
            this.pictureBox23.Size = new System.Drawing.Size(73, 96);
            this.pictureBox23.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox23.TabIndex = 10;
            this.pictureBox23.TabStop = false;
            // 
            // pictureBox24
            // 
            this.pictureBox24.Location = new System.Drawing.Point(331, 736);
            this.pictureBox24.Name = "pictureBox24";
            this.pictureBox24.Size = new System.Drawing.Size(73, 96);
            this.pictureBox24.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox24.TabIndex = 10;
            this.pictureBox24.TabStop = false;
            // 
            // pictureBox25
            // 
            this.pictureBox25.Location = new System.Drawing.Point(410, 736);
            this.pictureBox25.Name = "pictureBox25";
            this.pictureBox25.Size = new System.Drawing.Size(73, 96);
            this.pictureBox25.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox25.TabIndex = 10;
            this.pictureBox25.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("굴림", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label4.Location = new System.Drawing.Point(510, 217);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(18, 20);
            this.label4.TabIndex = 11;
            this.label4.Text = "/";
            // 
            // 총카드
            // 
            this.총카드.AutoSize = true;
            this.총카드.Font = new System.Drawing.Font("굴림", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.총카드.Location = new System.Drawing.Point(534, 217);
            this.총카드.Name = "총카드";
            this.총카드.Size = new System.Drawing.Size(0, 20);
            this.총카드.TabIndex = 11;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 855);
            this.Controls.Add(this.총카드);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.pictureBox25);
            this.Controls.Add(this.pictureBox24);
            this.Controls.Add(this.pictureBox23);
            this.Controls.Add(this.pictureBox22);
            this.Controls.Add(this.pictureBox21);
            this.Controls.Add(this.pictureBox20);
            this.Controls.Add(this.pictureBox19);
            this.Controls.Add(this.pictureBox18);
            this.Controls.Add(this.pictureBox17);
            this.Controls.Add(this.pictureBox16);
            this.Controls.Add(this.pictureBox15);
            this.Controls.Add(this.pictureBox14);
            this.Controls.Add(this.c122);
            this.Controls.Add(this.c82);
            this.Controls.Add(this.c42);
            this.Controls.Add(this.c112);
            this.Controls.Add(this.c72);
            this.Controls.Add(this.c32);
            this.Controls.Add(this.c102);
            this.Controls.Add(this.c62);
            this.Controls.Add(this.c22);
            this.Controls.Add(this.c92);
            this.Controls.Add(this.c52);
            this.Controls.Add(this.c12);
            this.Controls.Add(this.c121);
            this.Controls.Add(this.c81);
            this.Controls.Add(this.c41);
            this.Controls.Add(this.c111);
            this.Controls.Add(this.c71);
            this.Controls.Add(this.c31);
            this.Controls.Add(this.c101);
            this.Controls.Add(this.c61);
            this.Controls.Add(this.c21);
            this.Controls.Add(this.c91);
            this.Controls.Add(this.c51);
            this.Controls.Add(this.c11);
            this.Controls.Add(this.pictureBox13);
            this.Controls.Add(this.button16);
            this.Controls.Add(this.button15);
            this.Controls.Add(this.button14);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button12);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button11);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.checkBox12);
            this.Controls.Add(this.checkBox8);
            this.Controls.Add(this.checkBox4);
            this.Controls.Add(this.checkBox11);
            this.Controls.Add(this.checkBox7);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.checkBox10);
            this.Controls.Add(this.checkBox6);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox9);
            this.Controls.Add(this.checkBox5);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.라운드);
            this.Controls.Add(this.카드로테이션);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.카드갯수);
            this.Controls.Add(this.에너지);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox12);
            this.Controls.Add(this.pictureBox8);
            this.Controls.Add(this.pictureBox4);
            this.Controls.Add(this.pictureBox11);
            this.Controls.Add(this.pictureBox7);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.pictureBox10);
            this.Controls.Add(this.pictureBox6);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox9);
            this.Controls.Add(this.pictureBox5);
            this.Controls.Add(this.pictureBox1);
            this.Location = new System.Drawing.Point(1300, 0);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox11)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox13)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox14)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox15)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox16)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox17)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox18)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox19)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox20)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox21)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox22)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox23)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox24)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox25)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private PictureBox pictureBox4;
        private PictureBox pictureBox5;
        private PictureBox pictureBox6;
        private PictureBox pictureBox7;
        private PictureBox pictureBox8;
        private PictureBox pictureBox9;
        private PictureBox pictureBox10;
        private PictureBox pictureBox11;
        private PictureBox pictureBox12;
        private Label label1;
        private Label label2;
        private Label 에너지;
        private Label 카드갯수;
        private CheckBox checkBox1;
        private CheckBox checkBox2;
        private CheckBox checkBox3;
        private CheckBox checkBox4;
        private CheckBox checkBox5;
        private CheckBox checkBox6;
        private CheckBox checkBox7;
        private CheckBox checkBox8;
        private CheckBox checkBox9;
        private CheckBox checkBox10;
        private CheckBox checkBox11;
        private CheckBox checkBox12;
        private Label 카드로테이션;
        private Label label6;
        private Button button1;
        private void Button1_Click(object sender, EventArgs e)
        {
            if (엑시정보[0].in_hand > 0)
                엑시정보[0].in_hand--;
            else
                엑시정보[0].in_deck--;

            if (엑시정보[0].in_deck == -1) 엑시정보[0].in_deck = 1;
            card--; card_used++; UsedCard_in_round++; update();
            if (!checkBox1.Checked)
                energy--; update();
        }
        private Button button2;
        private void Button2_Click(object sender, EventArgs e)
        {
            if (엑시정보[1].in_hand > 0)
                엑시정보[1].in_hand--;
            else
                엑시정보[1].in_deck--; if (엑시정보[1].in_deck == -1) 엑시정보[1].in_deck = 1;
            card--; card_used++; UsedCard_in_round++; update();
            if (!checkBox2.Checked)
                energy--; update();
        }

        private Button button3;
        private void Button3_Click(object sender, EventArgs e)
        {
            if (엑시정보[2].in_hand > 0)
                엑시정보[2].in_hand--;
            else
                엑시정보[2].in_deck--;
            if (엑시정보[2].in_deck == -1) 엑시정보[2].in_deck = 1;
            card--; card_used++; UsedCard_in_round++; update();
            if (!checkBox3.Checked)
                energy--; update();
        }
        private Button button4;
        private void Button4_Click(object sender, EventArgs e)
        {
            if (엑시정보[3].in_hand > 0)
                엑시정보[3].in_hand--;
            else
                엑시정보[3].in_deck--; if (엑시정보[3].in_deck == -1) 엑시정보[3].in_deck = 1;
            card--; card_used++; UsedCard_in_round++; update();
            if (!checkBox4.Checked)
                energy--; update();
        }
        private Button button5;
        private void Button5_Click(object sender, EventArgs e)
        {
            if (엑시정보[4].in_hand > 0)
                엑시정보[4].in_hand--;
            else
                엑시정보[4].in_deck--; if (엑시정보[4].in_deck == -1) 엑시정보[4].in_deck = 1;
            card--; card_used++; UsedCard_in_round++; update();
            if (!checkBox5.Checked)
                energy--; update();
        }
        private Button button6;
        private void Button6_Click(object sender, EventArgs e)
        {
            if (엑시정보[5].in_hand > 0)
                엑시정보[5].in_hand--;
            else
                엑시정보[5].in_deck--; if (엑시정보[5].in_deck == -1) 엑시정보[5].in_deck = 1;
            card--; card_used++; UsedCard_in_round++; update();
            if (!checkBox6.Checked)
                energy--; update();
        }
        private Button button7;
        private void Button7_Click(object sender, EventArgs e)
        {
            if (엑시정보[6].in_hand > 0)
                엑시정보[6].in_hand--;
            else
                엑시정보[6].in_deck--; if (엑시정보[6].in_deck == -1) 엑시정보[6].in_deck = 1;
            card--; card_used++; UsedCard_in_round++; update();
            if (!checkBox7.Checked)
                energy--; update();
        }
        private Button button8;
        private void Button8_Click(object sender, EventArgs e)
        {
            if (엑시정보[7].in_hand > 0)
                엑시정보[7].in_hand--;
            else
                엑시정보[7].in_deck--; if (엑시정보[7].in_deck == -1) 엑시정보[7].in_deck = 1;
            card--; card_used++; UsedCard_in_round++; update();
            if (!checkBox8.Checked)
                energy--; update();
        }
        private Button button9;
        private void Button9_Click(object sender, EventArgs e)
        {
            if (엑시정보[8].in_hand > 0)
                엑시정보[8].in_hand--;
            else
                엑시정보[8].in_deck--; if (엑시정보[8].in_deck == -1) 엑시정보[8].in_deck = 1;
            card--; card_used++; UsedCard_in_round++; update();
            if (!checkBox9.Checked)
                energy--; update();
        }
        private Button button10;
        private void Button10_Click(object sender, EventArgs e)
        {
            if (엑시정보[9].in_hand > 0)
                엑시정보[9].in_hand--;
            else
                엑시정보[9].in_deck--; if (엑시정보[9].in_deck == -1) 엑시정보[9].in_deck = 1;
            card--; card_used++; UsedCard_in_round++; update();
            if (!checkBox10.Checked)
                energy--; update();
        }
        private Button button11;
        private void Button11_Click(object sender, EventArgs e)
        {
            if (엑시정보[10].in_hand > 0)
                엑시정보[10].in_hand--;
            else
                엑시정보[10].in_deck--; if (엑시정보[10].in_deck == -1) 엑시정보[10].in_deck = 1;
            card--; card_used++; UsedCard_in_round++; update();
            if (!checkBox11.Checked)
                energy--; update();
        }
        private Button button12;
        private void Button12_Click(object sender, EventArgs e)
        {
            if (엑시정보[11].in_hand > 0)
                엑시정보[11].in_hand--;
            else
                엑시정보[11].in_deck--; if (엑시정보[11].in_deck == -1) 엑시정보[11].in_deck = 1;
            card--; card_used++; UsedCard_in_round++; update();
            if (!checkBox12.Checked)
                energy--; update();
        }
        private Label label3;
        private Label 라운드;
        private Button button14;

        private void button14_Click(object sender, EventArgs e)
        {
            card_total -= 8;
            int 버릴카드갯수 = 8 - 엑시정보[0].in_deck - 엑시정보[1].in_deck - 엑시정보[2].in_deck - 엑시정보[3].in_deck;
            엑시정보[0].in_deck = 0; 엑시정보[1].in_deck = 0; 엑시정보[2].in_deck = 0; 엑시정보[3].in_deck = 0;
            엑시정보[0].in_hand = 0; 엑시정보[1].in_hand = 0; 엑시정보[2].in_hand = 0; 엑시정보[3].in_hand = 0;
            received_total -= 버릴카드갯수;
            card_used -= 버릴카드갯수;
            for (int i = 0; i < 4; i++)
                엑시정보[i].died = true;
            update(); rotate();
            button14.Enabled = false;
        }

        private Button button15;
        private void button15_Click(object sender, EventArgs e)
        {
            card_total -= 8;
            int 버릴카드갯수 = 8 - 엑시정보[4].in_deck - 엑시정보[5].in_deck - 엑시정보[6].in_deck - 엑시정보[7].in_deck;
            엑시정보[4].in_deck = 0; 엑시정보[5].in_deck = 0; 엑시정보[6].in_deck = 0; 엑시정보[7].in_deck = 0;
            엑시정보[4].in_hand = 0; 엑시정보[5].in_hand = 0; 엑시정보[6].in_hand = 0; 엑시정보[7].in_hand = 0;
            received_total -= 버릴카드갯수;
            card_used -= 버릴카드갯수;
            update(); rotate();
            for (int i = 4; i < 8; i++)
                엑시정보[i].died = true;
            button15.Enabled = false;
        }
        private Button button16;
        private void button16_Click(object sender, EventArgs e)
        {
            card_total -= 8;
            int 버릴카드갯수 = 8 - 엑시정보[8].in_deck - 엑시정보[9].in_deck - 엑시정보[10].in_deck - 엑시정보[11].in_deck;
            엑시정보[8].in_deck = 0; 엑시정보[9].in_deck = 0; 엑시정보[10].in_deck = 0; 엑시정보[11].in_deck = 0;
            엑시정보[8].in_hand = 0; 엑시정보[9].in_hand = 0; 엑시정보[10].in_hand = 0; 엑시정보[11].in_hand = 0;
            received_total -= 버릴카드갯수;
            card_used -= 버릴카드갯수;
            update(); rotate();
            for (int i = 8; i < 12; i++)
                엑시정보[i].died = true;
            button16.Enabled = false;

        }

        private PictureBox pictureBox13;
        private Label c11;
        private Label c12;
        private Label c21;
        private Label c22;
        private Label c31;
        private Label c32;
        private Label c41;
        private Label c42;
        private Label c51;
        private Label c61;
        private Label c71;
        private Label c81;
        private Label c52;
        private Label c62;
        private Label c72;
        private Label c82;
        private Label c91;
        private Label c101;
        private Label c111;
        private Label c121;
        private Label c92;
        private Label c102;
        private Label c112;
        private Label c122;
        private PictureBox pictureBox14;
        private PictureBox pictureBox15;
        private PictureBox pictureBox16;
        private PictureBox pictureBox17;
        private PictureBox pictureBox18;
        private PictureBox pictureBox19;
        private PictureBox pictureBox20;
        private PictureBox pictureBox21;
        private PictureBox pictureBox22;
        private PictureBox pictureBox23;
        private PictureBox pictureBox24;
        private PictureBox pictureBox25;
        private Label label4;
        private Label 총카드;
    }
}