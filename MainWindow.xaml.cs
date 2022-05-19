using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Drawing;
using System.Data;
using System.Threading;
using OpenCvSharp;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;

namespace WpfApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Console_block.Text = "Ready";
        }
        ////private void WindowsFormsHost_Initialized(object sender, EventArgs e)
        ////{
        ////    glControl.MakeCurrent();

        ////}
        ////private void glControl_Load(object sender, EventArgs e)
        ////{
        ////    GL.ClearColor(new Color4((byte)0, (byte)0, (byte)0, (byte)100));

        ////    GL.Enable(EnableCap.DepthTest);
        ////    GL.DepthFunc(DepthFunction.Lequal);
        ////    GL.ClearDepth(1000.0F);
        ////}
        //private void glControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        //{
        //    GL.Viewport(0, 0, glControl.Width, glControl.Height);
        //    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        //    GL.Enable(EnableCap.PointSmooth);

        //    GL.Enable(EnableCap.DepthTest);
        //    GL.DepthFunc(DepthFunction.Lequal);
        //    GL.ClearDepth(1000.0F);

        //    GL.ClearColor(new Color4((byte)0, (byte)0, (byte)0, (byte)100));
        //    GL.Begin(PrimitiveType.Points);
        //    GL.PointSize(2);
        //    GL.Color3((byte)255, (byte)0, (byte)0);
        //    GL.Vertex3((float)0, (float)0, (float)0);
        //    GL.End();
        //    GL.LineWidth(2);
        //    GL.Begin(PrimitiveType.Lines);
        //    GL.Color3((byte)255, (byte)0, (byte)0);
        //    GL.Vertex3((float)10, (float)0, (float)0);
        //    GL.Vertex3((float)0, (float)0, (float)0);
        //    GL.End();
        //    GL.Begin(PrimitiveType.Lines);
        //    GL.Color3((byte)0, (byte)255, (byte)0);
        //    GL.Vertex3((float)0, (float)10, (float)0);
        //    GL.Vertex3((float)0, (float)0, (float)0);
        //    GL.End();
        //    GL.Begin(PrimitiveType.Lines);
        //    GL.Color3((byte)0, (byte)0, (byte)255);
        //    GL.Vertex3((float)0, (float)0, (float)10);
        //    GL.Vertex3((float)0, (float)0, (float)0);
        //    GL.End();
        //    glControl.SwapBuffers();
        //}
        private void glControl_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {

        }
        private void glControl_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {

        }
        void segmentation()
        {
            try
            {
                SegmentedImagesList.Items.Clear();
                //CT_Scan.Source = BitmapFrame.Create(new Uri((String)ImagesList.Items[0]));
                //StreamWriter fout = new StreamWriter(data_path.Text);
                //fout.WriteLine("Сюда записались какие-то данные, но я пока не знаю как их вычленить");
                //fout.Close();
                //Console_block.Text += "\nНовое изображение загружено";
                int k = 0;
                foreach (string fileName in ImagesList.Items)
                {
                    Mat img1 = new Mat(fileName);
                    Mat hsv = new Mat(img1.Cols, img1.Rows, 8, 3);
                    Cv2.CvtColor(img1, hsv, ColorConversionCodes.BGR2HSV);
                    Mat[] splitedHsv = new Mat[3];
                    Cv2.Split(hsv, out splitedHsv);
                    CT_Scan_Orig.Source = BitmapFrame.Create(new Uri(fileName));
                    for (int x = 0; x < hsv.Rows; x++)
                    {
                        for (int y = 0; y < hsv.Cols; y++)
                        {
                            int H = (int)(splitedHsv[0].At<byte>(x, y));        // Тон
                            int S = (int)(splitedHsv[1].At<byte>(x, y));        // Интенсивность
                            int V = (int)(splitedHsv[2].At<byte>(x, y));        // Яркость
                            if (V >= Convert.ToInt32(LowScope.Text) && V < Convert.ToInt32(MidScope.Text))
                            {
                                img1.At<Vec3b>(x, y)[0] = 0;
                                img1.At<Vec3b>(x, y)[1] = 0;
                                img1.At<Vec3b>(x, y)[2] = 0;
                            }
                            else
                                if (V >= Convert.ToInt32(MidScope.Text) && V < Convert.ToInt32(TopScope.Text))
                            {
                                img1.At<Vec3b>(x, y)[0] = 150;
                                img1.At<Vec3b>(x, y)[1] = 150;
                                img1.At<Vec3b>(x, y)[2] = 150;
                            }
                            else
                            {
                                img1.At<Vec3b>(x, y)[0] = 255;
                                img1.At<Vec3b>(x, y)[1] = 255;
                                img1.At<Vec3b>(x, y)[2] = 255;
                            }
                        }
                    }
                    Cv2.ImWrite(scans_path.Text + @"\res" + k + ".tif", img1);
                    CT_Scan.Source = BitmapFrame.Create(new Uri((String)scans_path.Text + @"\res" + k + ".tif"));
                    SegmentedImagesList.Items.Add(scans_path.Text + @"\res" + k + ".tif");
                    Console_block.Text += "\nNew scan segmented";
                    num_of_scans.Text = "Elements: " + Convert.ToString(ImagesList.Items.Count);
                    k++;
                }
            }
            catch (Exception e1)
            {
                System.Windows.MessageBox.Show("Error! " + e1.Message);
            }
        }
        void filter()
        {
            int l = 0;
            int o = 0;
            string[] filenames = new string[SegmentedImagesList.Items.Count];
            foreach(string fileName in SegmentedImagesList.Items)
            {
                filenames[o] = fileName;
                o++;
            }
            SegmentedImagesList.Items.Clear();
            foreach(string fileName in filenames)
            {
                Mat img1 = new Mat(fileName);
                Mat hsv = new Mat(img1.Cols, img1.Rows, 8, 3);
                Cv2.CvtColor(img1, hsv, ColorConversionCodes.BGR2HSV);
                Mat[] splitedHsv = new Mat[3];
                Cv2.Split(hsv, out splitedHsv);
                CT_Scan_Orig.Source = BitmapFrame.Create(new Uri(fileName));
                for(int q = 0; q < 3; q++)
                {
                    for (int x = 0; x < hsv.Rows; x++)
                    {
                        for (int y = 0; y < hsv.Cols; y++)
                        {
                            if (x != 0 && y != 0 && x != img1.Cols - 1 && y != img1.Rows - 1)
                            {
                                int[][] filter_matrix = new int[3][];
                                int main_V = 0;
                                for (int i = 0; i < 3; i++)
                                {
                                    filter_matrix[i] = new int[3];
                                    for (int j = 0; j < 3; j++)
                                    {
                                        filter_matrix[i][j] = (int)(splitedHsv[2].At<byte>(x - 1 + i, y - 1 + j));
                                        main_V += filter_matrix[i][j];
                                    }
                                }
                                main_V /= 9;
                                if (main_V < 150)
                                {
                                    img1.At<Vec3b>(x, y)[0] = 0;
                                    img1.At<Vec3b>(x, y)[1] = 0;
                                    img1.At<Vec3b>(x, y)[2] = 0;
                                }
                                else if (main_V < 255)
                                {
                                    img1.At<Vec3b>(x, y)[0] = 150;
                                    img1.At<Vec3b>(x, y)[1] = 150;
                                    img1.At<Vec3b>(x, y)[2] = 150;
                                }
                                else
                                {
                                    img1.At<Vec3b>(x, y)[0] = 255;
                                    img1.At<Vec3b>(x, y)[1] = 255;
                                    img1.At<Vec3b>(x, y)[2] = 255;
                                }
                            }
                        }
                    }
                }
                Cv2.ImWrite(scans_path.Text + @"\res_filtered" + l + ".tif", img1);
                CT_Scan.Source = BitmapFrame.Create(new Uri((String)scans_path.Text + @"\res_filtered" + l + ".tif"));
                SegmentedImagesList.Items.Add(scans_path.Text + @"\res_filtered" + l + ".tif");
                Console_block.Text += "\nNew scan filtered";
                //num_of_scans.Text = "Elements: " + Convert.ToString(ImagesList.Items.Count);
                l++;
            }
        }
        void circle_mesh_creating()
        {
            try
            {
                StreamWriter fout = new StreamWriter(geometry_path.Text + @"\res" + ".geo");
                //fout.WriteLine("SetFactory(\"OpenCASCADE\");\n");
                int k = 0;
                height.Text = height.Text.Replace('.', ',');
                radius.Text = radius.Text.Replace('.', ',');
                float real_radius = (float)Convert.ToDouble(radius.Text);
                float diameter = real_radius * 2.0f;
                string string_diameter = Convert.ToString(diameter);
                float z = 0.0f + (k * ((float)Convert.ToDouble(height.Text) / (float)SegmentedImagesList.Items.Count));
                string string_z = z.ToString();
                string_z = string_z.Replace(',', '.');
                height.Text = height.Text.Replace(',', '.');
                radius.Text = radius.Text.Replace(',', '.');
                string_diameter = string_diameter.Replace(',', '.');
                fout.WriteLine("Point(1) = {" + radius.Text + ", 0, 0, 0.001};");
                fout.WriteLine("Point(2) = {" + radius.Text + ", " + radius.Text + ", 0, 0.001};");
                fout.WriteLine("Point(3) = {" + radius.Text + ", " + string_diameter + ", 0, 0.001};");
                fout.WriteLine("Point(4) = {" + radius.Text + ", 0, " + height.Text + ", 0.001};");
                fout.WriteLine("Point(5) = {" + radius.Text + ", " + radius.Text + ", " + height.Text + ", 0.001};");
                fout.WriteLine("Point(6) = {" + radius.Text + ", " + string_diameter + ", " + height.Text + ", 0.001};");
                fout.WriteLine("Circle(1) = {1, 2, 3};");
                fout.WriteLine("Circle(2) = {3, 2, 1};");
                fout.WriteLine("Circle(3) = {4, 5, 6};");
                fout.WriteLine("Circle(4) = {6, 5, 4};");
                fout.WriteLine("Spline(5) = {1, 4};");
                fout.WriteLine("Spline(6) = {3, 6};");
                fout.WriteLine("Line Loop(1) = {2, 1};");
                fout.WriteLine("Plane Surface(1) = {1};");
                fout.WriteLine("Line Loop(2) = {4, 3};");
                fout.WriteLine("Plane Surface(2) = {2};");
                fout.WriteLine("Line Loop(3) = {3, -6, -1, 5};");
                fout.WriteLine("Surface(3) = {3};");
                fout.WriteLine("Line Loop(4) = {4, -5, -2, 6};");
                fout.WriteLine("Surface(4) = {4};");
                fout.WriteLine("Surface Loop(1) = {3, 2, 4, 1};");
                fout.WriteLine("Volume(1) = {1};");
                //foreach (string fileName in SegmentedImagesList.Items)
                //{
                //    Mat img1 = new Mat(fileName);
                //    Mat hsv = new Mat(img1.Cols, img1.Rows, 8, 3);
                //    Cv2.CvtColor(img1, hsv, ColorConversionCodes.BGR2HSV);
                //    Mat[] splitedHsv = new Mat[3];
                //    Cv2.Split(hsv, out splitedHsv);
                //    float min_x = 0;
                //    float max_x = hsv.Rows;
                //    float y = hsv.Cols / 2.0f;
                    //for (int x = 0; x < hsv.Rows; x++)
                    //{ 
                    //    int V = (int)(splitedHsv[2].At<byte>(hsv.Cols, hsv.Rows));        // Яркость
                    //    if (V > Convert.ToInt32(LowScope.Text))
                    //    {
                    //        min_x = x; 
                    //        break;
                    //    }
                    //}
                    //for (int x = hsv.Rows; x > 0; x--)
                    //{
                    //    int V = (int)(splitedHsv[2].At<byte>(hsv.Cols, hsv.Rows));        // Яркость
                    //    if (V > Convert.ToInt32(LowScope.Text))
                    //    {
                    //        max_x = x;
                    //        break;
                    //    }
                    //}
                //    k++;
                //}
                fout.Close();
            }
            catch (Exception e2)
            {
                System.Windows.MessageBox.Show("Error! " + e2.Message);
            }
        }
        void cube_mesh_creating()
        {

        }
        //private void data_structuring(ref int points, ref int lines, ref int triangles, ref int tetrahedrons)
        //{
        //    try
        //    {
        //        StreamReader fin = new StreamReader((string)MeshesList.Items[0]);
        //        StreamWriter fout_x = new StreamWriter("nodes_x.txt");
        //        StreamWriter fout_y = new StreamWriter("nodes_y.txt");
        //        StreamWriter fout_z = new StreamWriter("nodes_z.txt");
        //        String help_str = "";
        //        int nodes = 0;
        //        do
        //        {
        //            help_str = fin.ReadLine();
        //        } while (help_str != "$Nodes");
        //        nodes = Convert.ToInt32(fin.ReadLine());
        //        for (int i = 0; i < nodes; i++)
        //        {
        //            help_str = fin.ReadLine();
        //            string[] subs = help_str.Split(' ');
        //            fout_x.WriteLine(subs[1]);
        //            fout_y.WriteLine(subs[2]);
        //            fout_z.WriteLine(subs[3]);
        //        }
        //        fout_x.Close();
        //        fout_y.Close();
        //        fout_z.Close();
        //        fin.ReadLine();
        //        fin.ReadLine();
        //        StreamWriter fout_type = new StreamWriter("element_type.txt");
        //        StreamWriter fout_first_node = new StreamWriter("first_node.txt");
        //        StreamWriter fout_second_node = new StreamWriter("second_node.txt");
        //        StreamWriter fout_third_node = new StreamWriter("third_node.txt");
        //        StreamWriter fout_fourth_node = new StreamWriter("fourth_node.txt");
        //        int elements = Convert.ToInt32(fin.ReadLine());
        //        points = 0;
        //        lines = 0;
        //        triangles = 0;
        //        tetrahedrons = 0;
        //        for (int i = 0; i < elements; i++)
        //        {
        //            help_str = fin.ReadLine();
        //            string[] subs = help_str.Split(' ');
        //            if (subs[1] == "15")
        //            {
        //                fout_type.WriteLine(subs[1]);
        //                fout_first_node.WriteLine(subs[5]);
        //                points++;
        //            }
        //            else if (subs[1] == "1")
        //            {
        //                fout_type.WriteLine(subs[1]);
        //                fout_first_node.WriteLine(subs[5]);
        //                fout_second_node.WriteLine(subs[6]);
        //                lines++;
        //            }
        //            else if (subs[1] == "2")
        //            {
        //                fout_type.WriteLine(subs[1]);
        //                fout_first_node.WriteLine(subs[5]);
        //                fout_second_node.WriteLine(subs[6]);
        //                fout_third_node.WriteLine(subs[7]);
        //                triangles++;
        //            }
        //            else if (subs[1] == "4")
        //            {
        //                fout_type.WriteLine(subs[1]);
        //                fout_first_node.WriteLine(subs[5]);
        //                fout_second_node.WriteLine(subs[6]);
        //                fout_third_node.WriteLine(subs[7]);
        //                fout_fourth_node.WriteLine(subs[8]);
        //                tetrahedrons++;
        //            }
        //        }
        //        fout_type.Close();
        //        fout_first_node.Close();
        //        fout_second_node.Close();
        //        fout_third_node.Close();
        //        fout_fourth_node.Close();
        //    }
        //    catch (Exception e)
        //    {
        //        System.Windows.MessageBox.Show("Error! " + e.Message);
        //    }
        //}
        //private void mesh_segmentation(ref int points, ref int lines, ref int triangles, ref int tetrahedrons)
        //{
        //    try
        //    {
        //        StreamReader fin_nodes_x = new StreamReader("nodes_x.txt");
        //        StreamReader fin_nodes_y = new StreamReader("nodes_y.txt");
        //        StreamReader fin_nodes_z = new StreamReader("nodes_z.txt");
        //        StreamReader fin_type = new StreamReader("type.txt");
        //        StreamReader fin_first_node = new StreamReader("first_node.txt");
        //        StreamReader fin_second_node = new StreamReader("second_node.txt");
        //        StreamReader fin_third_node = new StreamReader("third_node.txt");
        //        StreamReader fin_fourth_node = new StreamReader("fourth_node.txt");
        //        for (int i = 0; i < points; i++)
        //        {
        //            String help_str = fin_first_node.ReadLine();
        //            help_str.Replace('.', ',');
        //            int first_node = Convert.ToInt32(help_str);
        //            for (int j = 0; j < first_node; j++)
        //            {
        //                help_str = fin_nodes_z.ReadLine();
        //            }
        //            fin_nodes_z.DiscardBufferedData();

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        System.Windows.MessageBox.Show("Error! " + e.Message);
        //    }
        //}
        //private void old_mesh_segmentation()
        //{
        //    try
        //    {
        //        fin_msh = new StreamReader((string)MeshesList.Items[0]);
        //        string help_str = "";
        //        do
        //        {
        //            help_str = fin_msh.ReadLine();
        //        } while (help_str != "$Nodes");
        //        nodes = Convert.ToInt32(fin_msh.ReadLine());
        //        x_nodes = new double[nodes];
        //        y_nodes = new double[nodes];
        //        z_nodes = new double[nodes];
        //        for (int i = 0; i < nodes; i++)
        //        {
        //            help_str = fin_msh.ReadLine();
        //            string[] subs = help_str.Split(' ');
        //            subs[1] = subs[1].Replace('.', ',');
        //            subs[2] = subs[2].Replace('.', ',');
        //            subs[3] = subs[3].Replace('.', ',');
        //            x_nodes[i] = Convert.ToDouble(subs[1]);
        //            y_nodes[i] = Convert.ToDouble(subs[2]);
        //            z_nodes[i] = Convert.ToDouble(subs[3]);
        //        }
        //        fin_msh.ReadLine();
        //        fin_msh.ReadLine();
        //        elements = Convert.ToInt32(fin_msh.ReadLine());
        //        element_type = new int[elements];
        //        first_node = new int[elements];
        //        second_node = new int[elements];
        //        third_node = new int[elements];
        //        fourth_node = new int[elements];
        //        centre = new double[elements][];
        //        for (int i = 0; i < elements; i++)
        //        {
        //            centre[i] = new double[3];
        //        }
        //        int[] tag = new int[elements];
        //        for (int i = 0; i < elements; i++)
        //        {
        //            string[] subs = fin_msh.ReadLine().Split(' ');
        //            if (Convert.ToInt32(subs[1]) == 1)
        //            {
        //                element_type[i] = 1;
        //                tag[i] = Convert.ToInt32(subs[3]);
        //                first_node[i] = Convert.ToInt32(subs[5]);
        //                second_node[i] = Convert.ToInt32(subs[6]);
        //                centre[i][0] = (x_nodes[first_node[i] - 1] + x_nodes[second_node[i] - 1]) / 2;
        //                centre[i][1] = (y_nodes[first_node[i] - 1] + y_nodes[second_node[i] - 1]) / 2;
        //                centre[i][2] = (z_nodes[first_node[i] - 1] + z_nodes[second_node[i] - 1]) / 2;
        //            }
        //            else if (Convert.ToInt32(subs[1]) == 2)
        //            {
        //                element_type[i] = 2;
        //                tag[i] = Convert.ToInt32(subs[3]);
        //                first_node[i] = Convert.ToInt32(subs[5]);
        //                second_node[i] = Convert.ToInt32(subs[6]);
        //                third_node[i] = Convert.ToInt32(subs[7]);
        //                centre[i][0] = (x_nodes[first_node[i] - 1] + x_nodes[second_node[i] - 1] + x_nodes[third_node[i] - 1]) / 3;
        //                centre[i][1] = (y_nodes[first_node[i] - 1] + y_nodes[second_node[i] - 1] + y_nodes[third_node[i] - 1]) / 3;
        //                centre[i][2] = (z_nodes[first_node[i] - 1] + z_nodes[second_node[i] - 1] + z_nodes[third_node[i] - 1]) / 3;
        //            }
        //            else if (Convert.ToInt32(subs[1]) == 15)
        //            {
        //                element_type[i] = 15;
        //                tag[i] = Convert.ToInt32(subs[3]);
        //                first_node[i] = Convert.ToInt32(subs[5]);
        //                centre[i][0] = x_nodes[first_node[i] - 1];
        //                centre[i][1] = y_nodes[first_node[i] - 1];
        //                centre[i][2] = z_nodes[first_node[i] - 1];
        //            }
        //            else if (Convert.ToInt32(subs[1]) == 4)
        //            {
        //                element_type[i] = 4;
        //                tag[i] = Convert.ToInt32(subs[3]);
        //                first_node[i] = Convert.ToInt32(subs[5]);
        //                second_node[i] = Convert.ToInt32(subs[6]);
        //                third_node[i] = Convert.ToInt32(subs[7]);
        //                fourth_node[i] = Convert.ToInt32(subs[8]);
        //                centre[i][0] = (x_nodes[first_node[i] - 1] + x_nodes[second_node[i] - 1] + x_nodes[third_node[i] - 1] + x_nodes[fourth_node[i] - 1]) / 4;
        //                centre[i][1] = (y_nodes[first_node[i] - 1] + y_nodes[second_node[i] - 1] + y_nodes[third_node[i] - 1] + y_nodes[fourth_node[i] - 1]) / 4;
        //                centre[i][2] = (z_nodes[first_node[i] - 1] + z_nodes[second_node[i] - 1] + z_nodes[third_node[i] - 1] + z_nodes[fourth_node[i] - 1]) / 4;
        //            }
        //        }
        //        fin_msh.Close();
        //        StreamWriter fout = new StreamWriter(meshes_path.Text + @"\resmesh" + ".msh");
        //        fout.WriteLine("$MeshFormat");
        //        fout.WriteLine("2.2 0 8");
        //        fout.WriteLine("$EndMeshFormat");
        //        fout.WriteLine("$Nodes");
        //        fout.WriteLine(nodes);
        //        for (int i = 0; i < nodes; i++)
        //        {
        //            string x_node = Convert.ToString(x_nodes[i]);
        //            string y_node = Convert.ToString(y_nodes[i]);
        //            string z_node = Convert.ToString(z_nodes[i]);
        //            x_node = x_node.Replace(',', '.');
        //            y_node = y_node.Replace(',', '.');
        //            z_node = z_node.Replace(',', '.');
        //            fout.WriteLine((i + 1) + " " + x_node + " " + y_node + " " + z_node);
        //            fout.WriteLine("$EndNodes");
        //            fout.WriteLine("$Elements");
        //            fout.WriteLine(elements);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        System.Windows.MessageBox.Show("Error! " + e.Message);
        //    }
        //}
        void mesh_segmentation()
        {
            try
            {
                int nodes = 0;
                StreamReader fin = new StreamReader((string)MeshesList.Items[0]);
                string help_str = "";
                do
                {
                    help_str = fin.ReadLine();
                } while (help_str != "$Nodes");
                nodes = Convert.ToInt32(fin.ReadLine());
                double[] x_nodes = new double[nodes];
                double[] y_nodes = new double[nodes];
                double[] z_nodes = new double[nodes];
                for (int i = 0; i < nodes; i++)
                {
                    help_str = fin.ReadLine();
                    string[] subs = help_str.Split(' ');
                    subs[1] = subs[1].Replace('.', ',');
                    subs[2] = subs[2].Replace('.', ',');
                    subs[3] = subs[3].Replace('.', ',');
                    x_nodes[i] = Convert.ToDouble(subs[1]);
                    y_nodes[i] = Convert.ToDouble(subs[2]);
                    z_nodes[i] = Convert.ToDouble(subs[3]);
                }
                fin.ReadLine();
                fin.ReadLine();
                int elements = Convert.ToInt32(fin.ReadLine());
                int borders = 0;
                int[] element_type = new int[elements];
                int[] first_node = new int[elements];
                int[] second_node = new int[elements];
                int[] third_node = new int[elements];
                int[] fourth_node = new int[elements];
                double[][] centre = new double[elements][];
                for (int i = 0; i < elements; i++)
                {
                    centre[i] = new double[3];
                }
                int[] tag = new int[elements];
                for (int i = 0; i < elements; i++)
                {
                    string[] subs = fin.ReadLine().Split(' ');
                    if (Convert.ToInt32(subs[1]) == 1)
                    {
                        element_type[i] = 1;
                        tag[i] = Convert.ToInt32(subs[3]);
                        first_node[i] = Convert.ToInt32(subs[5]);
                        second_node[i] = Convert.ToInt32(subs[6]);
                        centre[i][0] = (x_nodes[first_node[i] - 1] + x_nodes[second_node[i] - 1]) / 2;
                        centre[i][1] = (y_nodes[first_node[i] - 1] + y_nodes[second_node[i] - 1]) / 2;
                        centre[i][2] = (z_nodes[first_node[i] - 1] + z_nodes[second_node[i] - 1]) / 2;
                    }
                    else if (Convert.ToInt32(subs[1]) == 2)
                    {
                        element_type[i] = 2;
                        tag[i] = Convert.ToInt32(subs[3]);
                        first_node[i] = Convert.ToInt32(subs[5]);
                        second_node[i] = Convert.ToInt32(subs[6]);
                        third_node[i] = Convert.ToInt32(subs[7]);
                        centre[i][0] = (x_nodes[first_node[i] - 1] + x_nodes[second_node[i] - 1] + x_nodes[third_node[i] - 1]) / 3;
                        centre[i][1] = (y_nodes[first_node[i] - 1] + y_nodes[second_node[i] - 1] + y_nodes[third_node[i] - 1]) / 3;
                        centre[i][2] = (z_nodes[first_node[i] - 1] + z_nodes[second_node[i] - 1] + z_nodes[third_node[i] - 1]) / 3;
                    }
                    else if (Convert.ToInt32(subs[1]) == 15)
                    {
                        element_type[i] = 15;
                        tag[i] = Convert.ToInt32(subs[3]);
                        first_node[i] = Convert.ToInt32(subs[5]);
                        centre[i][0] = x_nodes[first_node[i] - 1];
                        centre[i][1] = y_nodes[first_node[i] - 1];
                        centre[i][2] = z_nodes[first_node[i] - 1];
                    }
                    else if (Convert.ToInt32(subs[1]) == 4)
                    {
                        element_type[i] = 4;
                        tag[i] = Convert.ToInt32(subs[3]);
                        first_node[i] = Convert.ToInt32(subs[5]);
                        second_node[i] = Convert.ToInt32(subs[6]);
                        third_node[i] = Convert.ToInt32(subs[7]);
                        fourth_node[i] = Convert.ToInt32(subs[8]);
                        centre[i][0] = (x_nodes[first_node[i] - 1] + x_nodes[second_node[i] - 1] + x_nodes[third_node[i] - 1] + x_nodes[fourth_node[i] - 1]) / 4;
                        centre[i][1] = (y_nodes[first_node[i] - 1] + y_nodes[second_node[i] - 1] + y_nodes[third_node[i] - 1] + y_nodes[fourth_node[i] - 1]) / 4;
                        centre[i][2] = (z_nodes[first_node[i] - 1] + z_nodes[second_node[i] - 1] + z_nodes[third_node[i] - 1] + z_nodes[fourth_node[i] - 1]) / 4;
                    }
                }
                fin.Close();
                fin.DiscardBufferedData();
                StreamWriter fout = new StreamWriter(meshes_path.Text + @"\resmesh" + ".msh");
                fout.WriteLine("$MeshFormat");
                fout.WriteLine("2.2 0 8");
                fout.WriteLine("$EndMeshFormat");
                fout.WriteLine("$Nodes");
                fout.WriteLine(nodes);
                for (int i = 0; i < nodes; i++)
                {
                    string x_node = Convert.ToString(x_nodes[i]);
                    string y_node = Convert.ToString(y_nodes[i]);
                    string z_node = Convert.ToString(z_nodes[i]);
                    x_node = x_node.Replace(',', '.');
                    y_node = y_node.Replace(',', '.');
                    z_node = z_node.Replace(',', '.');
                    fout.WriteLine((i + 1) + " " + x_node + " " + y_node + " " + z_node);
                }
                fout.WriteLine("$EndNodes");
                fout.WriteLine("$Elements");
                fout.WriteLine(elements);
                int k = 0;
                foreach (string fileName in SegmentedImagesList.Items)
                {
                    Mat img1 = new Mat(fileName);
                    Mat hsv = new Mat(img1.Cols, img1.Rows, 8, 3);
                    Cv2.CvtColor(img1, hsv, ColorConversionCodes.BGR2HSV);
                    Mat[] splitedHsv = new Mat[3];
                    Cv2.Split(hsv, out splitedHsv);
                    height.Text = height.Text.Replace('.', ',');
                    radius.Text = radius.Text.Replace('.', ',');
                    float z_scan = (float)Convert.ToDouble(height.Text);
                    float scan_radius = (float)Convert.ToDouble(radius.Text);

                    for (int i = 0; i < elements; i++)
                    {
                        float h_centre = (float)centre[i][2];
                        float scan_pos = k * z_scan / SegmentedImagesList.Items.Count;
                        if (Math.Abs(scan_pos - h_centre) < z_scan / SegmentedImagesList.Items.Count / 2)
                        //if (Math.Abs(centre[i][2] - z_scan) < (z_scan / (2 * SegmentedImagesList.Items.Count)))
                        {
                            if (element_type[i] == 1)
                            {
                                int V1 = (int)(splitedHsv[2].At<byte>((int)(x_nodes[first_node[i] - 1] / (scan_radius * 2) * img1.Cols), (int)(y_nodes[first_node[i] - 1] / (scan_radius * 2) * img1.Rows)));
                                int V2 = (int)(splitedHsv[2].At<byte>((int)(x_nodes[second_node[i] - 1] / (scan_radius * 2) * img1.Cols), (int)(y_nodes[second_node[i] - 1] / (scan_radius * 2) * img1.Rows)));
                                int V3 = (int)(splitedHsv[2].At<byte>((int)(centre[i][0] / (scan_radius * 2) * img1.Cols), (int)(centre[i][1] / (scan_radius * 2) * img1.Rows)));
                                int V = V1 + V2 + V3;
                                if (V <= 255)
                                {
                                    tag[i] = 0;
                                }
                                else if (V >= 300 && V != 405 && V <= 450)
                                {
                                    tag[i] = 1;
                                }
                                else if (V != 405)
                                {
                                    tag[i] = 4;
                                }
                                //else
                                //{
                                //    tag[i] = 5;
                                //    borders++;
                                //}
                            }
                            else if (element_type[i] == 2)
                            {
                                int V1 = (int)(splitedHsv[2].At<byte>((int)(x_nodes[first_node[i] - 1] / (scan_radius * 2) * img1.Cols), (int)(y_nodes[first_node[i] - 1] / (scan_radius * 2) * img1.Rows)));
                                int V2 = (int)(splitedHsv[2].At<byte>((int)(x_nodes[second_node[i] - 1] / (scan_radius * 2) * img1.Cols), (int)(y_nodes[second_node[i] - 1] / (scan_radius * 2) * img1.Rows)));
                                int V3 = (int)(splitedHsv[2].At<byte>((int)(centre[i][0] / (scan_radius * 2) * img1.Cols), (int)(centre[i][1] / (scan_radius * 2) * img1.Rows)));
                                int V4 = (int)(splitedHsv[2].At<byte>((int)(x_nodes[third_node[i] - 1] / (scan_radius * 2) * img1.Cols), (int)(y_nodes[third_node[i] - 1] / (scan_radius * 2) * img1.Rows)));
                                int V = V1 + V2 + V3 + V4;
                                if (V <= 255)
                                {
                                    tag[i] = 0;
                                }
                                else if (V > 300 && V <= 600 && V != 510)
                                {
                                    tag[i] = 1;
                                }
                                else if (V != 810 && V != 300 && V != 510)
                                {
                                    tag[i] = 4;
                                }
                                //else
                                //{
                                //    tag[i] = 5;
                                //    borders++;
                                //}
                            }
                            else if (element_type[i] == 15)
                            {
                                int V = (int)(splitedHsv[2].At<byte>((int)(x_nodes[first_node[i] - 1] / (scan_radius * 2) * img1.Cols), (int)(y_nodes[first_node[i] - 1] / (scan_radius * 2) * img1.Rows)));
                                if (V == 0)
                                {
                                    tag[i] = 0;
                                }
                                else if (V == 150)
                                {
                                    tag[i] = 1;
                                }
                                else if (V == 255)
                                {
                                    tag[i] = 4;
                                }
                                //else
                                //{
                                //    tag[i] = 5;
                                //}
                            }
                            else if (element_type[i] == 4)
                            {
                                int V1 = (int)(splitedHsv[2].At<byte>((int)(x_nodes[first_node[i] - 1] / (scan_radius * 2) * img1.Cols), (int)(y_nodes[first_node[i] - 1] / (scan_radius * 2) * img1.Rows)));
                                int V2 = (int)(splitedHsv[2].At<byte>((int)(x_nodes[second_node[i] - 1] / (scan_radius * 2) * img1.Cols), (int)(y_nodes[second_node[i] - 1] / (scan_radius * 2) * img1.Rows)));
                                int V3 = (int)(splitedHsv[2].At<byte>((int)(centre[i][0] / (scan_radius * 2) * img1.Cols), (int)(centre[i][1] / (scan_radius * 2) * img1.Rows)));
                                int V4 = (int)(splitedHsv[2].At<byte>((int)(x_nodes[third_node[i] - 1] / (scan_radius * 2) * img1.Cols), (int)(y_nodes[third_node[i] - 1] / (scan_radius * 2) * img1.Rows)));
                                int V5 = (int)(splitedHsv[2].At<byte>((int)(x_nodes[fourth_node[i] - 1] / (scan_radius * 2) * img1.Cols), (int)(y_nodes[fourth_node[i] - 1] / (scan_radius * 2) * img1.Rows)));
                                int V = V1 + V2 + V3 + V4 + V5;
                                if (V <= 510 && V != 450)
                                {
                                    tag[i] = 0;
                                }
                                else if (V >= 765 && V != 855 && V != 960)
                                {
                                    tag[i] = 4;
                                }
                                else
                                {
                                    tag[i] = 1;
                                }
                            }
                        }
                    }
                    k++;
                }
                for (int i = 0; i < elements; i++)
                {
                    if (element_type[i] == 15)
                    {
                        fout.WriteLine((i + 1) + " " + element_type[i] + " " + 2 + " " + tag[i] + " " + tag[i] + " " + first_node[i]);
                    }
                    else if (element_type[i] == 1)
                    {
                        fout.WriteLine((i + 1) + " " + element_type[i] + " " + 2 + " " + tag[i] + " " + tag[i] + " " + first_node[i] + " " + second_node[i]);
                    }
                    else if (element_type[i] == 2)
                    {
                        fout.WriteLine((i + 1) + " " + element_type[i] + " " + 2 + " " + tag[i] + " " + tag[i] + " " + first_node[i] + " " + second_node[i] + " " + third_node[i]);
                    }
                    else if (element_type[i] == 4)
                    {
                        fout.WriteLine((i + 1) + " " + element_type[i] + " " + 2 + " " + tag[i] + " " + tag[i] + " " + first_node[i] + " " + second_node[i] + " " + third_node[i] + " " + fourth_node[i]);
                    }
                }
                fout.WriteLine("$EndElements");
                fout.Close();
                float border_percent = (float)borders * 100.0f / (float)elements;
                Console_block.Text += "\nNew mesh segmented\nBorders: " + border_percent + "%";
                SegmentedMeshesList.Items.Add(meshes_path.Text + @"\resmesh" + ".msh");
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Error! " + e.Message);
            }
        }
        private void convert_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                segmentation();
                filter();
                circle_mesh_creating();
            }
            catch (Exception e2)
            {
                System.Windows.MessageBox.Show("Error! " + e2.Message);
            }
        }
        private void btnOpenScans_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "Image files (*.png;*.jpg)|*.png;*.jpeg;*.jpg;*.tif;*.tiff;*.bmp|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (openFileDialog.ShowDialog() == true)
                {
                    ImagesList.Items.Clear();
                    foreach (string filename in openFileDialog.FileNames)
                        ImagesList.Items.Add(System.IO.Path.GetFullPath(filename));
                }
            }
            catch (Exception e2)
            {
                System.Windows.MessageBox.Show("Error! " + e2.Message);
            }
        }
        private void btnSaveScans_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderBrowserDialog saveFileDialog = new FolderBrowserDialog();
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    scans_path.Text = saveFileDialog.SelectedPath;
                }
            }
            catch (Exception e3)
            {
                System.Windows.MessageBox.Show("Error! " + e3.Message);
            }
        }
        private void btnOpenMeshes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "Mesh files (*.msh)|*.msh|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string filename in openFileDialog.FileNames)
                        MeshesList.Items.Add(System.IO.Path.GetFullPath(filename));
                }
            }
            catch (Exception e2)
            {
                System.Windows.MessageBox.Show("Error! " + e2.Message);
            }
        }
        private void btnSaveMeshes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderBrowserDialog saveFileDialog = new FolderBrowserDialog();
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    meshes_path.Text = saveFileDialog.SelectedPath;
                }
            }
            catch (Exception e3)
            {
                System.Windows.MessageBox.Show("Error! " + e3.Message);
            }
        }
        private void mesh_button_Click(object sender, RoutedEventArgs e)
        {
            mesh_segmentation();
        }
        private void btnSaveGeo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderBrowserDialog saveFileDialog = new FolderBrowserDialog();
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    geometry_path.Text = saveFileDialog.SelectedPath;
                }
            }
            catch (Exception e3)
            {
                System.Windows.MessageBox.Show("Error! " + e3.Message);
            }
        }

        private void ImagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CT_Scan_Orig.Source = BitmapFrame.Create(new Uri((string)ImagesList.Items[ImagesList.SelectedIndex]));
        }

        private void SegmentedImagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CT_Scan.Source = BitmapFrame.Create(new Uri((string)SegmentedImagesList.Items[SegmentedImagesList.SelectedIndex]));
        }
    }
}