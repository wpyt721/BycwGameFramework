using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class GameDataMgr : Singleton<GameDataMgr>
{
    private Dictionary<string, Dictionary<string, object>> dataDic = null;

    public void Init()
    {
        this.dataDic = new Dictionary<string, Dictionary<string, object>>();    
    }
    
    
    public T GetConfigData<T>(string key, string fileName = null)
    {
        Type setT = typeof(T);
        if (fileName == null)
        {
            fileName = setT.Name;
        }

        if (!dataDic.ContainsKey(fileName))
        {
            ReadConfigData<T>(fileName);
        }
        Dictionary<string, object> objDic = dataDic[fileName];

        if (!objDic.ContainsKey(key))
        {
            throw new Exception("no this config");
        }

        return (T)(objDic[key]);
    }

    public List<T> GetConfigDatas<T>(string fileName, bool hasPriKey = true) where T : GameDataMgr
    {
        List<T> returnList = new List<T>();
        Type setT = typeof(T);
        if (fileName == null)
        {
            fileName = setT.Name;
        }

        if (!dataDic.ContainsKey(fileName))
        {
            ReadConfigData<T>(fileName);
        }
        Dictionary<string, object> objDic = dataDic[fileName];

        foreach (KeyValuePair<string, object> kvp in objDic)
        {
            returnList.Add((T)kvp.Value);
        }
       
        return returnList;
    }

    public void ReadConfigData<T>(string fileName = null, bool hasPriKey = true) 
    {
        // T obj = Activator.CreateInstance<T>();
        if (fileName == null)
        {
            // fileName = obj.getFilePath();
            fileName = typeof(T).Name;
        }
        
        string path = "Datas/" + fileName + ".csv";
        string getString = (ResMgr.Instance.LoadAssetSync<TextAsset>(path) as TextAsset).text;
        CsvReaderByString csv = new CsvReaderByString(getString);
        
        Dictionary<string, object> objDic = new Dictionary<string, object>();
        
        FieldInfo[] fis = new FieldInfo[csv.ColCount];
        for (int i = 1; i < csv.ColCount + 1; i++)
            fis[i - 1] = typeof(T).GetField(csv[3, i]);

        int index = 0;
        for (int rowNum = 4; rowNum <= csv.RowCount; rowNum++)
        {
            T configObj = Activator.CreateInstance<T>();
            for (int i = 0; i < fis.Length; i++)
            {
                string fieldValue = csv[rowNum, i + 1];
                object setValue = new object();

                switch (fis[i].FieldType.ToString())
                {
                    case "System.Int32":
                        setValue = int.Parse(fieldValue);
                        break;
                    case "System.Int64":
                        setValue = long.Parse(fieldValue);
                        break;
                    case "System.String":
                        setValue = fieldValue;
                        break;
                    case "System.Single":
                        try
                        {
                            setValue = float.Parse(fieldValue);
                        }
                        catch (Exception e)
                        {
                            setValue = 0.0f;
                        }
                        break;
                    default:
                        Debug.Log("error data type:");
                        break;
                }
                fis[i].SetValue(configObj, setValue);
                if (hasPriKey && (fis[i].Name == "key" || fis[i].Name == "ID"))
                {
                    //只检测key和id的值，然后添加到objDic
                    objDic.Add(setValue.ToString(), configObj);
                }
            }

            if (!hasPriKey)
            {
                objDic.Add(index.ToString(), configObj);
            }
            index++;
        }
        dataDic.Add(fileName, objDic);
    }
}
