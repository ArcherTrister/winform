using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;

namespace WinFormUI
{
    public class IDCardAPI
    {
        //���ȣ�����ͨ�ýӿ�


        [DllImport("sdtapi.dll")]
        public static extern int SDT_ClosePort(int iPortID);

        [DllImport("sdtapi.dll")]
        public static extern int SDT_GetCOMBaud();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iPortID"></param>
        /// <param name="StrSAMID">16���ֽ�</param>
        /// <param name="iIfOpen"></param>
        /// <returns></returns>
        [DllImport("sdtapi.dll")]

        public static extern int SDT_GetSAMID(int iPortID, byte[] StrSAMID, int iIfOpen);

        [DllImport("sdtapi.dll")]
        public static extern int SDT_GetSAMIDToStr(int iPortID, byte[] pcSAMID, int iIfOpen);

        [DllImport("sdtapi.dll")]
        public static extern int SDT_GetSAMStatus(int iPortID, int iIfOpen);

        [DllImport("sdtapi.dll")]
        public static extern int SDT_OpenPort(int iPortID);

        [DllImport("sdtapi.dll")]
        public static extern int SDT_ReadBaseMsg(int iPortID, byte[] pucCHMsg, ref int puiCHMsgLen, byte[] pucPHMsg, ref int puiPHMsgLen, int iIfOpen);

        //int STDCALL SDT_ReadBaseMsg(int iPortID,unsigned char * pucCHMsg,unsigned int *	puiCHMsgLen,unsigned char * pucPHMsg,unsigned int  *puiPHMsgLen,int iIfOpen);


        [DllImport("sdtapi.dll")]
        public static extern int SDT_ReadBaseMsgToFile(int iPortID, string fileName1, ref int puiCHMsgLen, string fileName2, ref int puiPHMsgLen, int iIfOpen);


        [DllImport("sdtapi.dll")]
        public static extern int SDT_ReadNewAppMsg(int iPortID, ref byte pucAppMsg, ref int puiAppMsgLen, int iIfOpen);


        [DllImport("sdtapi.dll")]
        public static extern int SDT_ResetSAM(int iPortID, int iIfOpen);


        [DllImport("sdtapi.dll")]
        public static extern int SDT_SelectIDCard(int iPortID, ref int pucSN, int iIfOpen);

        [DllImport("sdtapi.dll")]
        public static extern int SDT_SetCOMBaud(int iComID, int uiCurrBaud, int uiSetBaud);

        [DllImport("sdtapi.dll")]
        public static extern int SDT_SetMaxRFByte(int iPortID, byte ucByte, int iIfOpen);

        [DllImport("sdtapi.dll")]
        public static extern int SDT_StartFindIDCard(int iPortID, ref int pucIIN, int iIfOpen);


        [DllImport("WltRS.dll")]
        public static extern int GetBmp(string file_name, int intf);

        [DllImport("CMIDAPI.dll")]
        public static extern string ReadIDInfo(int iport);



        public delegate void De_ReadICCardComplete(EDZ objEDZ);
        public event De_ReadICCardComplete ReadICCardComplete;
        private EDZ objEDZ = new EDZ();
        private int EdziIfOpen = 1;               //�Զ����ش���
        int EdziPortID;

        public int InitDevice()
        {
            bool bUsbPort = false;

            int intOpenPortRtn = 0;

            for (int iPort = 1001; iPort <= 1016; iPort++)
            {
                intOpenPortRtn = SDT_OpenPort(iPort);
                if (intOpenPortRtn == 144)
                {
                    EdziPortID = iPort;
                    bUsbPort = true;
                    break;
                }
            }
            //��⴮�ڵĻ�������
            if (!bUsbPort)
            {
                for (int iPort = 1; iPort <= 2; iPort++)
                {
                    intOpenPortRtn = SDT_OpenPort(iPort);
                    if (intOpenPortRtn == 144)
                    {
                        EdziPortID = iPort;
                        bUsbPort = false;
                        break;
                    }
                }
            }
            if (intOpenPortRtn != 144)
            {
                throw new Exception("�˿ڴ�ʧ�ܣ�������Ӧ�Ķ˿ڻ����������Ӷ�������");
                //return 0;
            }

            return EdziPortID;

        }


        public EDZ ReadICCard(int iPort)
        {
            bool bUsbPort = false;
            int intOpenPortRtn = 0;
            int rtnTemp = 0;
            int pucIIN = 0;
            int pucSN = 0;
            int puiCHMsgLen = 0;
            int puiPHMsgLen = 0;

            objEDZ = new EDZ();


            if (iPort > 1000)
            {
                bUsbPort = true;
            }
            intOpenPortRtn = SDT_OpenPort(iPort);

            if (intOpenPortRtn != 144)
            {
                throw new Exception("�˿ڴ�ʧ�ܣ�������Ӧ�Ķ˿ڻ����������Ӷ�������");
            }
            //�ҿ�
            rtnTemp = SDT_StartFindIDCard(EdziPortID, ref pucIIN, EdziIfOpen);
            if (rtnTemp != 159)
            {
                rtnTemp = SDT_StartFindIDCard(EdziPortID, ref pucIIN, EdziIfOpen);  //���ҿ�
                if (rtnTemp != 159)
                {
                    rtnTemp = SDT_ClosePort(EdziPortID);
                    throw new Exception("δ�ſ����߿�δ�źã������·ſ���");
                }
            }

            //ѡ��
            rtnTemp = SDT_SelectIDCard(EdziPortID, ref pucSN, EdziIfOpen);
            if (rtnTemp != 144)
            {
                rtnTemp = SDT_SelectIDCard(EdziPortID, ref pucSN, EdziIfOpen);  //��ѡ��
                if (rtnTemp != 144)
                {
                    rtnTemp = SDT_ClosePort(EdziPortID);
                    throw new Exception("����ʧ�ܣ�");
                }
            }
            //ע�⣬������û�������Ӧ�ó���ǰĿ¼�Ķ�дȨ��
            FileInfo objFile = new FileInfo("wz.txt");
            if (objFile.Exists)
            {
                objFile.Attributes = FileAttributes.Normal;
                objFile.Delete();
            }
            objFile = new FileInfo("zp.bmp");
            if (objFile.Exists)
            {
                objFile.Attributes = FileAttributes.Normal;
                objFile.Delete();
            }
            objFile = new FileInfo("zp.wlt");
            if (objFile.Exists)
            {
                objFile.Attributes = FileAttributes.Normal;
                objFile.Delete();
            }
            rtnTemp = SDT_ReadBaseMsgToFile(EdziPortID, "D:\\wz.txt", ref puiCHMsgLen, "D:\\zp.wlt", ref puiPHMsgLen, EdziIfOpen);
            if (rtnTemp != 144)
            {
                rtnTemp = SDT_ClosePort(EdziPortID);
                throw new Exception("����ʧ�ܣ�");
                //return "����ʧ�ܣ�";
            }
            
            FileInfo f = new FileInfo("D:\\wz.txt");
            FileStream fs = f.OpenRead();
            byte[] bt = new byte[fs.Length];
            fs.Read(bt, 0, (int)fs.Length);
            fs.Close();

            string str = UnicodeEncoding.Unicode.GetString(bt);

            objEDZ.Name = UnicodeEncoding.Unicode.GetString(bt, 0, 30).Trim();
            objEDZ.Sex_Code = UnicodeEncoding.Unicode.GetString(bt, 30, 2).Trim();
            objEDZ.NATION_Code = UnicodeEncoding.Unicode.GetString(bt, 32, 4).Trim();
            string strBird = UnicodeEncoding.Unicode.GetString(bt, 36, 16).Trim();
            objEDZ.BIRTH = Convert.ToDateTime(strBird.Substring(0, 4) + "��" + strBird.Substring(4, 2) + "��" + strBird.Substring(6) + "��");
            objEDZ.ADDRESS = UnicodeEncoding.Unicode.GetString(bt, 52, 70).Trim();
            objEDZ.IDC = UnicodeEncoding.Unicode.GetString(bt, 122, 36).Trim();
            objEDZ.REGORG = UnicodeEncoding.Unicode.GetString(bt, 158, 30).Trim();
            string strTem = UnicodeEncoding.Unicode.GetString(bt, 188, bt.GetLength(0) - 188).Trim();
            objEDZ.STARTDATE = Convert.ToDateTime(strTem.Substring(0, 4) + "��" + strTem.Substring(4, 2) + "��" + strTem.Substring(6, 2) + "��");
            strTem = strTem.Substring(8);
            if (strTem.Trim() != "����")
            {
                objEDZ.ENDDATE = Convert.ToDateTime(strTem.Substring(0, 4) + "��" + strTem.Substring(4, 2) + "��" + strTem.Substring(6, 2) + "��");
            }
            else
            {
                objEDZ.ENDDATE = DateTime.MaxValue;
            }
            
          
            //ReadICCardComplete(objEDZ);
            return objEDZ;
        }


    }

 
    public class EDZ
    {
        private SortedList lstMZ = new SortedList();
        private string _Name;   //����
        private string _Sex_Code;   //�Ա����
        private string _Sex_CName;   //�Ա�
        private string _IDC;      //���֤����
        private string _NATION_Code;   //�������
        private string _NATION_CName;   //����
        private DateTime _BIRTH;     //��������
        private string _ADDRESS;    //סַ
        private string _REGORG;     //ǩ������
        private DateTime _STARTDATE;    //���֤��Ч��ʼ����
        private DateTime _ENDDATE;    //���֤��Ч��������
        private string _Period_Of_Validity_Code;   //��Ч���޴��룬���ԭ��ϵͳ����Ϊ��һ��֤���ǣ����������������ֶΣ�����֤���Ѿ�û����
        private string _Period_Of_Validity_CName;   //��Ч����
        

        public EDZ()
        {
            lstMZ.Add("01", "����");
            lstMZ.Add("02", "�ɹ���");
            lstMZ.Add("03", "����");
            lstMZ.Add("04", "����");
            lstMZ.Add("05", "ά�����");
            lstMZ.Add("06", "����");
            lstMZ.Add("07", "����");
            lstMZ.Add("08", "׳��");
            lstMZ.Add("09", "������");
            lstMZ.Add("10", "������");
            lstMZ.Add("11", "����");
            lstMZ.Add("12", "����");
            lstMZ.Add("13", "����");
            lstMZ.Add("14", "����");
            lstMZ.Add("15", "������");
            lstMZ.Add("16", "������");
            lstMZ.Add("17", "��������");
            lstMZ.Add("18", "����");
            lstMZ.Add("19", "����");
            lstMZ.Add("20", "������");
            lstMZ.Add("21", "����");
            lstMZ.Add("22", "���");
            lstMZ.Add("23", "��ɽ��");
            lstMZ.Add("24", "������");
            lstMZ.Add("25", "ˮ��");
            lstMZ.Add("26", "������");
            lstMZ.Add("27", "������");
            lstMZ.Add("28", "������");
            lstMZ.Add("29", "�¶�������");
            lstMZ.Add("30", "����");
            lstMZ.Add("31", "�ﺲ����");
            lstMZ.Add("32", "������");
            lstMZ.Add("33", "Ǽ��");
            lstMZ.Add("34", "������");
            lstMZ.Add("35", "������");
            lstMZ.Add("36", "ë����");
            lstMZ.Add("37", "������");
            lstMZ.Add("38", "������");
            lstMZ.Add("39", "������");
            lstMZ.Add("40", "������");
            lstMZ.Add("41", "��������");
            lstMZ.Add("42", "ŭ��");
            lstMZ.Add("43", "���α����");
            lstMZ.Add("44", "����˹��");
            lstMZ.Add("45", "���¿���");
            lstMZ.Add("46", "�°���");
            lstMZ.Add("47", "������");
            lstMZ.Add("48", "ԣ����");
            lstMZ.Add("49", "����");
            lstMZ.Add("50", "��������");
            lstMZ.Add("51", "������");
            lstMZ.Add("52", "���״���");
            lstMZ.Add("53", "������");
            lstMZ.Add("54", "�Ű���");
            lstMZ.Add("55", "�����");
            lstMZ.Add("56", "��ŵ��");
            lstMZ.Add("57", "����");
            lstMZ.Add("98", "������뼮");
        }

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        public string Sex_Code
        {
            get { return _Sex_Code; }
            set
            {
                _Sex_Code = value;
                switch (value)
                {
                    case "1":
                        Sex_CName = "��";
                        break;
                    case "2":
                        Sex_CName = "Ů";
                        break;
                }
            }
        }
        public string Sex_CName
        {
            get { return _Sex_CName; }
            set { _Sex_CName = value; }
        }
        public string IDC
        {
            get { return _IDC; }
            set { _IDC = value; }
        }
        public string NATION_Code
        {
            get { return _NATION_Code; }
            set
            {
                _NATION_Code = value;
                if (lstMZ.Contains(value))
                    NATION_CName = lstMZ[value].ToString();
            }
        }
        public string NATION_CName
        {
            get { return _NATION_CName; }
            set { _NATION_CName = value; }
        }
        public DateTime BIRTH
        {
            get { return _BIRTH; }
            set { _BIRTH = value; }
        }
        public string ADDRESS
        {
            get { return _ADDRESS; }
            set { _ADDRESS = value; }
        }
        public string REGORG
        {
            get { return _REGORG; }
            set { _REGORG = value; }
        }
        public DateTime STARTDATE
        {
            get { return _STARTDATE; }
            set { _STARTDATE = value; }
        }
        public DateTime ENDDATE
        {
            get { return _ENDDATE; }
            set
            {
                _ENDDATE = value;
                if (_ENDDATE == DateTime.MaxValue)
                {
                    _Period_Of_Validity_Code = "3";
                    _Period_Of_Validity_CName = "����";
                }
                else
                {
                    if (_STARTDATE != DateTime.MinValue)
                    {
                        switch (value.AddDays(1).Year - _STARTDATE.Year)
                        {
                            case 5:
                                _Period_Of_Validity_Code = "4";
                                _Period_Of_Validity_CName = "5��";
                                break;
                            case 10:
                                _Period_Of_Validity_Code = "1";
                                _Period_Of_Validity_CName = "10��";
                                break;
                            case 20:
                                _Period_Of_Validity_Code = "2";
                                _Period_Of_Validity_CName = "20��";
                                break;
                        }
                    }
                }

            }
        }
        public string Period_Of_Validity_Code
        {
            get { return _Period_Of_Validity_Code; }
            set { _Period_Of_Validity_Code = value; }
        }
        public string Period_Of_Validity_CName
        {
            get { return _Period_Of_Validity_CName; }
            set { _Period_Of_Validity_CName = value; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("����:{0}", this.Name).AppendLine();
            sb.AppendFormat("���֤����:{0}", this.IDC).AppendLine();
            return sb.ToString();
        }
    }
}