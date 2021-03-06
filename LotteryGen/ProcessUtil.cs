﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryGen
{
    public static class ProcessUtil
    {
        public static void WritePersonConfig(string destination,
            Dictionary<string, Person> excels)
        {
            Console.WriteLine(nameof(ProcessUtil.WritePersonConfig));
            Console.WriteLine();

            var filename = Path.Combine(destination, "Person.txt");
            const string Template = "{{ L\"{0}\", IDR_PERSON{1}, LR\"~({2})~\" }},";
            using (var file = File.OpenWrite(filename))
            using (var writer = new StreamWriter(file))
            {
                var id = 1;
                foreach (var kv in excels)
                {
                    writer.WriteLine(string.Format(Template, kv.Key, id, kv.Value.Quote));
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.WriteLine($"{id}/{excels.Count}...");
                    ++id;
                }
            }

            Console.WriteLine("Done.");
        }

        internal static void GenerateWallJson(string dest, List<Person> list)
        {
            dest = Path.Combine(dest, "new", "data.js");
            var json = JsonConvert.SerializeObject(list.Select(x => new
            {
                姓名 = x.Name, 
                总结 = x.Quote
            }), Formatting.Indented);
            File.WriteAllText(dest, "var data = " + json);
        }

        internal static void CopyImagesToNames(string dest, List<Person> persons)
        {
            Console.WriteLine(nameof(ProcessUtil.CopyImagesToNames));
            Console.WriteLine();

            var newFolder = Path.Combine(dest, "new");
            Directory.CreateDirectory(newFolder);

            foreach (var person in persons)
            {
                var oldFile = Path.Combine(dest, $"{person.Id + 1}.jpg");
                var newFile = Path.Combine(newFolder, $"{person.Name}.jpg");
                File.Copy(oldFile, newFile, true);

                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.WriteLine($"{person.Id + 1}/{persons.Count}...");
            }

            Console.WriteLine("Done");
        }

        internal static void WriteNews(string dest, Dictionary<string, Person> excels)
        {
            File.WriteAllText(Path.Combine(dest, "news.txt"), string.Join(", ", excels
                .Where(x => x.Value.IsNew)
                .Select(x => x.Value.Id)));
        }

        public static void ProcessImages(
            string destination,
            List<string> names,
            Dictionary<string, string> images)
        {
            Console.WriteLine(nameof(ProcessUtil.ProcessImages));
            Console.WriteLine();

            var id = 0;
            const int MinEdge = 300;
            foreach (var name in names)
            {
                ++id;
                if (!images.ContainsKey(name)) continue;

                var oldFile = images[name];
                using (var oldImg = Image.FromFile(oldFile, true))
                {
                    if (Array.IndexOf(oldImg.PropertyIdList, 274) > -1)
                    {
                        var orientation = (int)oldImg.GetPropertyItem(274).Value[0];
                        switch (orientation)
                        {
                            case 1:
                                // No rotation required.
                                break;
                            case 2:
                                oldImg.RotateFlip(RotateFlipType.RotateNoneFlipX);
                                break;
                            case 3:
                                oldImg.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                break;
                            case 4:
                                oldImg.RotateFlip(RotateFlipType.Rotate180FlipX);
                                break;
                            case 5:
                                oldImg.RotateFlip(RotateFlipType.Rotate90FlipX);
                                break;
                            case 6:
                                oldImg.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                break;
                            case 7:
                                oldImg.RotateFlip(RotateFlipType.Rotate270FlipX);
                                break;
                            case 8:
                                oldImg.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                break;
                        }
                    }

                    var rate = 1.0 * Math.Min(oldImg.Width, oldImg.Height) / MinEdge;
                    using (var newImg = new Bitmap((int)(oldImg.Width / rate), (int)(oldImg.Height / rate)))
                    using (var g = Graphics.FromImage(newImg))
                    {
                        g.InterpolationMode = InterpolationMode.High;
                        g.DrawImage(oldImg, 0, 0, newImg.Width, newImg.Height);
                        newImg.Save($"{Path.Combine(destination, id.ToString())}.jpg", ImageFormat.Jpeg);

                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.WriteLine($"{id}/{names.Count}...");
                    }
                }
            }

            Console.WriteLine("Done");
        }

        public static void CheckConflicts(
            IEnumerable<string> names,
            Dictionary<string, string> images)
        {
            var excelsExcept = names.Except(images.Keys).ToList();
            if (excelsExcept.Count > 0)
            {
                Console.WriteLine("Names existed in excel, but not in images: ");
                Console.WriteLine(string.Join("\t", excelsExcept));
            }

            var imagesExcept = images.Keys.Except(names).ToList();
            if (imagesExcept.Count > 0)
            {
                Console.WriteLine("Names existed in images, but not in excel: ");
                Console.WriteLine(string.Join("\t", imagesExcept));
            }
        }
    }
}
