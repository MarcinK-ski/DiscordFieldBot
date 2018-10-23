using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace DiscordFieldBot
{
    class Helper
    {
        public static void BinarySerializeObject(string path, object obj)
        {
            using (StreamWriter streamWriter = new StreamWriter(path))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                try
                {
                    binaryFormatter.Serialize(streamWriter.BaseStream, obj);
                }
                catch (SerializationException ex)
                {
                    throw new SerializationException(((object)ex).ToString() + "\n" + ex.Source);
                }
            }
        }

        public static object BinaryDeserializeObject(string path)
        {
            if (File.Exists(path))
            {
                using (StreamReader streamReader = new StreamReader(path))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    object obj;
                    try
                    {
                        obj = binaryFormatter.Deserialize(streamReader.BaseStream);
                    }
                    catch (SerializationException ex)
                    {
                        throw new SerializationException(((object)ex).ToString() + "\n" + ex.Source);
                    }
                    return obj;
                }
            }
            else
            {
                return null;
            }
        }

        public static void ConsoleWriteExceptionInfo(Exception ex)
        {
            Console.WriteLine(ex.Source);
            Console.WriteLine(ex.TargetSite);
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.InnerException);
            Console.WriteLine(ex.StackTrace);
        }
    }
}
