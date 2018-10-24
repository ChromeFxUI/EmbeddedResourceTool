using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace EmbeddedResourceTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string csproj = AppDomain.CurrentDomain.BaseDirectory + args[0];
            string distStr = AppDomain.CurrentDomain.BaseDirectory + args[1];

            if (!Directory.Exists(distStr))
            {
                Console.WriteLine("未找到资源文件：" + distStr);
            }

            if(!File.Exists(csproj))
            {
                Console.WriteLine("未找到项目文件:" + csproj);
            }

            //初始化一个xml实例
            XmlDocument xml = new XmlDocument();

            //导入指定xml文件
            xml.Load(csproj);

            var projectElement = xml.GetElementsByTagName("Project");

            for (int i = 0; i < projectElement.Count; i++)
            {
                var item = projectElement[i];
                if (item.ChildNodes.Count > 1)
                {
                    for (int j = 0; j < item.ChildNodes.Count; j++)
                    {
                        var node = item.ChildNodes[j];
                        if (node.Name == "ItemGroup")
                        {
                            for (int m = 0; m < node.ChildNodes.Count; m++)
                            {
                                var mnode = node.ChildNodes[m];
                                if (mnode.Name == "EmbeddedResource")
                                {
                                    if (mnode.Attributes.Count == 1)
                                    {
                                        var cur = mnode.Attributes.GetNamedItem("Include");
                                        if (cur.Value.Contains("dist"))
                                        {
                                            node.RemoveChild(mnode);
                                            xml.Save(csproj);
                                            m--;
                                        }
                                    }
                                    Console.WriteLine(mnode.OuterXml);
                                }
                            }
                            if (node.ChildNodes.Count == 0)
                            {
                                item.RemoveChild(node);
                                xml.Save(csproj);
                                j--;
                            }
                            
                        }
                    }

                    DirectoryInfo directoryInfo = new DirectoryInfo(distStr);
                    var info = directoryInfo.GetFileSystemInfos();

                    var itemGroupElement = xml.CreateElement("", "ItemGroup", "");

                    itemGroupElement = SearchFile(xml, itemGroupElement, directoryInfo);

                    item.AppendChild(itemGroupElement);
                    xml.Save(csproj);
                }
            }

           

            xml.Save(csproj);

           var content =  File.ReadAllText(csproj);
            content = content.Replace("xmlns=\"\"", "");
            File.WriteAllText(csproj, content);
        }

        public static XmlElement SearchFile(XmlDocument xml, XmlElement node, DirectoryInfo directoryInfo)
        {
            var curNode = node;
            var info = directoryInfo.GetFileSystemInfos();
            if (info.Count() == 0)
            {
                return curNode;
            }
            else
            {
                foreach (var curInfo in info)
                {
                    if (curInfo.Attributes == FileAttributes.Directory)
                    {
                        curNode = SearchFile(xml,curNode, new DirectoryInfo(curInfo.FullName));
                    }
                    else
                    {
                        var next = xml.CreateElement("EmbeddedResource", "EmbeddedResource", "");

                        var curPath = curInfo.FullName.Replace(AppDomain.CurrentDomain.BaseDirectory,"");

                        next.SetAttribute("Include", curPath);
                        curNode.AppendChild(next);


                    }
                }
            }
            return curNode;
        }
    }
}
