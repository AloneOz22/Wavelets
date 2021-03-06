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
        void segmentation()
        {
            try
            {
                SegmentedImagesList.Items.Clear();
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
