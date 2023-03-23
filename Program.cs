using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NPKUnpacker
{
    class Program
    {
        static string 文件路径 = @"D:\单机dnf\朽叶DNF客户端 V1.0531\台服DNF客户端\ImagePacks2\sprite(ex).NPK";
        static string 文件路径2 = @"D:\单机dnf\朽叶DNF客户端 V1.0531\台服DNF客户端\ImagePacks2\sprite.NPK";
        static string 文件路径3 = @"D:\单机dnf\朽叶DNF客户端 V1.0531\台服DNF客户端\ImagePacks2\sprite_character_swordman_equipment_avatar_skin(Transformed).npk";
        static Stream 流;
        static byte[] 缓存字节数组;

        static string NPK文件头;
        static int IMG文件总数;

        static Dictionary<int, Dictionary<int, string>> IMG文件索引表 = new Dictionary<int, Dictionary<int, string>>();

        static byte[] NPK校验位 = new byte[32];

        static byte[][] IMG文件序列;

        static IMGV2文件接口[] IMGV2文件数组;
        static void Main(string[] args)
        {
            //读取文件流
            流 = new FileStream(文件路径3,FileMode.Open);
            
            //读取文件头
            缓存字节数组 = new byte[16];
            流.Read(缓存字节数组,0,16);
            NPK文件头 = Encoding.UTF8.GetString(缓存字节数组);            
            Console.WriteLine("NPK文件头:"+NPK文件头);

            //读取IMG文件总数
            缓存字节数组 = new byte[4];
            流.Read(缓存字节数组, 0, 4);
            IMG文件总数 = BitConverter.ToInt32(缓存字节数组,0);
            Console.WriteLine("IMG文件总数:" + IMG文件总数);

            //读取IMG文件索引表
            for (int i = 0; i < IMG文件总数; i++)
            {
                Console.WriteLine("-----------------------------------------");
                缓存字节数组 = new byte[4];
                流.Read(缓存字节数组, 0, 4);
                int 地址偏移量 = BitConverter.ToInt32(缓存字节数组, 0);
                Console.WriteLine("地址偏移量:" + 地址偏移量);

                缓存字节数组 = new byte[4];
                流.Read(缓存字节数组, 0, 4);
                int IMG文件大小 = BitConverter.ToInt32(缓存字节数组, 0);
                Console.WriteLine("IMG文件大小" + IMG文件大小);

                缓存字节数组 = new byte[256];
                流.Read(缓存字节数组, 0, 256);
                
                string 异或字符串 = "puchikon@neople dungeon and fighter DNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNFDNF ";
                for (int t = 0; t < 256; t++)
                {
                    缓存字节数组[t] ^= (byte)异或字符串[t];
                }
                string IMG文件名称 = Encoding.UTF8.GetString(缓存字节数组);
                IMG文件名称 = IMG文件名称.Substring(0,IMG文件名称.IndexOf(".img")+4);
                Console.WriteLine("IMG文件名称"+IMG文件名称);
                Console.WriteLine($"当前字节流位置{流.Position}");

                IMG文件索引表.Add(地址偏移量, new Dictionary<int, string>() { { IMG文件大小, IMG文件名称 } });
            }
            IMG文件索引表.DumpAll();

            //读取NPK校验位
            流.Read(NPK校验位,0,32);

            //读取IMG文件序列
            IMG文件序列 = new byte[IMG文件总数][];
            for (int i = 0; i < IMG文件序列.Length; i++)
            {
                流.Position = IMG文件索引表.GetKey(i);
                IMG文件序列[i] = new byte[IMG文件索引表.GetValueKey(i)];
                流.Read(IMG文件序列[i],0,IMG文件索引表.GetValueKey(i));
            }
            //IMG文件序列.DumpAll();

            //读取IMG文件，以IMGV2格式
            IMGV2文件数组 = new IMGV2文件接口[IMG文件序列.Length];
            for (int i = 0; i < IMGV2文件数组.Length; i++)
            {
                IMGV2文件数组[i] = new IMGV2(IMG文件序列[i]);
            }

            //测试输出
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("测试部分：读取NPK第0个IMG的信息：");

            Console.WriteLine("文件头:"+IMGV2文件数组[0].取文件头());
            Console.WriteLine("索引表大小:"+IMGV2文件数组[0].取索引表大小());
            Console.WriteLine("保留:" + IMGV2文件数组[0].取保留());
            Console.WriteLine("版本号:" + IMGV2文件数组[0].取版本号());
            Console.WriteLine("索引表数目:" + IMGV2文件数组[0].取索引表数目());


            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("测试部分：读取NPK第0个IMG的坐标");

            //测试输出
            for (int i = 0; i < IMGV2文件数组[0].取索引表数目(); i++)
            {
                IMG索引文件接口 当前索引文件 = IMGV2文件数组[0].取IMG索引文件(i);
                while (当前索引文件.取类型()== (int)IMG索引类型枚举.指向型)
                {
                    当前索引文件 = IMGV2文件数组[0].取IMG索引文件(当前索引文件.取指向帧号());
                }
                Console.WriteLine($"当前帧号：{i}");
                Console.WriteLine(当前索引文件.取颜色系统().ToColorSystemName());
                Console.Write(当前索引文件.取x坐标());
                Console.Write("\t");
                Console.WriteLine(当前索引文件.取y坐标());
                Console.Write(当前索引文件.取图像宽());
                Console.Write("\t");
                Console.WriteLine(当前索引文件.取图像高());
                Console.Write(当前索引文件.取帧域宽());
                Console.Write("\t");
                Console.WriteLine(当前索引文件.取帧域高());
            }
            Console.ReadLine();
        }
    }
    public static class 扩展方法类_IMG文件索引表
    {
        //调试输出
        public static void DumpAll(this Dictionary<int, Dictionary<int, string>> _dic)
        {
            List<int> keys = new List<int>();
            List<int> values_keys = new List<int>();
            List<string> values_values = new List<string>();
            foreach (KeyValuePair<int,Dictionary<int,string>> item0 in _dic)
            {
                keys.Add(item0.Key);
                foreach (KeyValuePair<int,string> item1 in item0.Value)
                {
                    values_keys.Add(item1.Key);
                    values_values.Add(item1.Value);
                }
            }
            for (int i = 0; i < _dic.Count; i++)
            {
                string result = "";
                result += keys[i].ToString();
                result += "\t";
                result += values_keys[i].ToString();
                result += "\t";
                result += values_values[i];
                Console.WriteLine(result);
            }
        }
        //取主键
        public static int GetKey(this Dictionary<int, Dictionary<int, string>> _dic,int index)
        {
            List<int> keys = new List<int>();
            foreach (KeyValuePair<int, Dictionary<int, string>> item0 in _dic)
            {
                keys.Add(item0.Key);
            }
            return keys[index];
        }
        //取子键
        public static int GetValueKey(this Dictionary<int, Dictionary<int, string>> _dic, int index)
        {            
            List<int> values_keys = new List<int>();
            foreach (KeyValuePair<int, Dictionary<int, string>> item0 in _dic)
            {
                foreach (KeyValuePair<int, string> item1 in item0.Value)
                {
                    values_keys.Add(item1.Key);
                }
            }
            return values_keys[index];
        }
        //取子值
        public static string GetValueValue(this Dictionary<int, Dictionary<int, string>> _dic, int index)
        {
            List<string> values_values = new List<string>();
            foreach (KeyValuePair<int, Dictionary<int, string>> item0 in _dic)
            {
                foreach (KeyValuePair<int, string> item1 in item0.Value)
                {
                    values_values.Add(item1.Value);
                }
            }
            return values_values[index];
        }
    }
    public static class 扩展方法类_IMG文件序列
    {
        //调试输出
        public static void DumpAll(this byte[][] _array)
        {
            for (int i = 0; i < _array.Length; i++)
            {
                Console.WriteLine("当前IMG文件序号："+i);
                Console.WriteLine(BitConverter.ToString(_array[i]));
            }
        }
    }
    public static class 扩展方法类_字节数组
    {
        //复制数组的一部分
        public static byte[] SubArray(this byte[] _array,int startIndex,int length)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = _array[i + startIndex];
            }
            return result;
        }
    }
    public static class 扩展方法类_整数型
    {
        //整数转字符串，指颜色系统
        public static string ToColorSystemName(this int _value)
        {
            if (_value == (int)PNG文件颜色系统枚举.ARGB8888)
            {
                return "ARGB8888";
            }
            if (_value == (int)PNG文件颜色系统枚举.ARGB4444)
            {
                return "ARGB4444";
            }
            if (_value == (int)PNG文件颜色系统枚举.ARGB1555)
            {
                return "ARGB1555";
            }
            else
            {
                return "";
            }
        }
    }
    public interface IMGV2文件接口
    {
        string 取文件头();
        int 取索引表大小();
        int 取保留();
        int 取版本号();
        int 取索引表数目();
        IMG索引文件接口 取IMG索引文件(int _index);
    }
    public interface IMG文件接口 : IMGV2文件接口
    { }
    public interface IMG图片型索引文件接口
    {
        int 取颜色系统();
        int 取压缩状态();
        int 取图像宽();
        int 取图像高();
        int 取图像大小();
        int 取x坐标();
        int 取y坐标();
        int 取帧域宽();
        int 取帧域高();
    }
    public interface IMG指向型索引文件接口
    {
        int 取类型();
        int 取指向帧号();
    }
    public interface IMG索引文件接口 : IMG图片型索引文件接口, IMG指向型索引文件接口
    { }
    public enum PNG文件颜色系统枚举 : int
    {
        ARGB8888 = 0x10,
        ARGB4444 = 0x0F,
        ARGB1555 = 0x0E
    }
    public enum PNG压缩状态枚举 : int
    { 
        未压缩 = 0x05,
        ZLib压缩 = 0x06
    }
    public enum IMG索引类型枚举 : int 
    {
        指向型 = 0x11,
        图片型
    }
    public class 指向型索引项类 : IMG索引文件接口
    {
        int 类型;
        int 指向帧号;
        public 指向型索引项类(int _类型,int _指向帧号)
        {
            类型=_类型;
            指向帧号=_指向帧号;
        }
        void 置类型(int _value) { 类型 = _value; }
        void 置指向帧号(int _value) { 指向帧号 = _value; }

        public int 取类型(){return 类型;}
        public int 取指向帧号() { return 指向帧号; }

        public int 取颜色系统(){ return 0; }
        public int 取压缩状态(){ return 0; }
        public int 取图像宽(){ return 0; }
        public int 取图像高(){ return 0; }
        public int 取图像大小(){ return 0; }
        public int 取x坐标(){ return 0; }
        public int 取y坐标(){ return 0; }
        public int 取帧域宽(){ return 0; }
        public int 取帧域高(){ return 0; }
    }
    public class 图片型索引项 : IMG索引文件接口
    {
        int 颜色系统;
        int 压缩状态;
        int 图像宽 ;
        int 图像高 ;
        int 图像大小;
        int x坐标 ;
        int y坐标 ;
        int 帧域宽 ;
        int 帧域高 ;
        public 图片型索引项(
            int _颜色系统,
            int _压缩状态,
            int _图像宽,
            int _图像高,
            int _图像大小,
            int _x坐标,
            int _y坐标,
            int _帧域宽,
            int _帧域高)
        {
            颜色系统 = _颜色系统;
            压缩状态 = _压缩状态;
            图像宽 = _图像宽;
            图像高 = _图像高;
            图像大小 = _图像大小;
            x坐标 = _x坐标;
            y坐标 = _y坐标;
            帧域宽 = _帧域宽;
            帧域高 = _帧域高;
        }
        public int 取颜色系统() { return 颜色系统; }
        public int 取压缩状态() { return 压缩状态; }
        public int 取图像宽() { return 图像宽; }
        public int 取图像高() { return 图像高; }
        public int 取图像大小() { return 图像大小; }
        public int 取x坐标() { return x坐标; }
        public int 取y坐标() { return y坐标; }
        public int 取帧域宽() { return 帧域宽; }
        public int 取帧域高() { return 帧域高; }

        public int 取类型() { return 0; }
        public int 取指向帧号() { return 0; }

    }
    public class IMGV2: IMGV2文件接口
    {
        string 文件头 = "";
        int 索引表大小 = 0;
        int 保留 = 0;
        int 版本号 = 0;
        int 索引表数目 = 0;
        IMG索引文件接口[] IMG索引文件接口数组;

        byte[][] 贴图数据数组;
        public IMGV2(byte[] _data)
        {
            int index = 0;
            文件头 = Encoding.UTF8.GetString(_data.SubArray(index, 16));
            index += 16;
            索引表大小 = BitConverter.ToInt32(_data.SubArray(index, 4),0);
            index += 4;
            保留 = BitConverter.ToInt32(_data.SubArray(index, 4), 0);
            index += 4;
            版本号 = BitConverter.ToInt32(_data.SubArray(index, 4), 0);
            index += 4;
            索引表数目 = BitConverter.ToInt32(_data.SubArray(index, 4), 0);
            index += 4;
            IMG索引文件接口数组 = new IMG索引文件接口[索引表数目];
            for (int i = 0; i < 索引表数目; i++)
            {
                int 类型 = BitConverter.ToInt32(_data.SubArray(index, 4), 0);
                if (类型 == (int)IMG索引类型枚举.指向型)
                {
                    index += 4;
                    int 指向帧号 = BitConverter.ToInt32(_data.SubArray(index, 4), 0);
                    IMG索引文件接口数组[i] = new 指向型索引项类(类型, 指向帧号);
                    index += 4;
                }
                else
                {
                    int 颜色系统 = BitConverter.ToInt32(_data.SubArray(index, 4), 0);
                    index += 4;
                    int 压缩状态 = BitConverter.ToInt32(_data.SubArray(index, 4), 0);
                    index += 4;
                    int 图像宽 = BitConverter.ToInt32(_data.SubArray(index, 4), 0);
                    index += 4;
                    int 图像高 = BitConverter.ToInt32(_data.SubArray(index, 4), 0);
                    index += 4;
                    int 图像大小 = BitConverter.ToInt32(_data.SubArray(index, 4), 0);
                    index += 4;
                    int x坐标 = BitConverter.ToInt32(_data.SubArray(index, 4), 0);
                    index += 4;
                    int y坐标 = BitConverter.ToInt32(_data.SubArray(index, 4), 0);
                    index += 4;
                    int 帧域宽 = BitConverter.ToInt32(_data.SubArray(index, 4), 0);
                    index += 4;
                    int 帧域高 = BitConverter.ToInt32(_data.SubArray(index, 4), 0);
                    IMG索引文件接口数组[i] = new 图片型索引项(
                        颜色系统,
                        压缩状态,
                        图像宽,
                        图像高,
                        图像大小,
                        x坐标,
                        y坐标,
                        帧域宽,
                        帧域高);
                    index += 4;
                }
            }
            贴图数据数组 = new byte[索引表数目][];
            for (int i = 0; i < 贴图数据数组.Length; i++)
            {
                if (IMG索引文件接口数组[i].取类型() == (int)IMG索引类型枚举.指向型)
                {
                    贴图数据数组[i] = new byte[0];
                }
                else
                {
                    贴图数据数组[i] = _data.SubArray(index, IMG索引文件接口数组[i].取图像大小());
                    index += IMG索引文件接口数组[i].取图像大小();
                }
            }
        }
        public string 取文件头(){return 文件头;}
        public int 取索引表大小(){return 索引表大小;}
        public int 取保留(){return 保留;}
        public int 取版本号(){return 版本号;}
        public int 取索引表数目(){return 索引表数目;}
        public IMG索引文件接口 取IMG索引文件(int _index){ return IMG索引文件接口数组[_index]; }
    }
}
