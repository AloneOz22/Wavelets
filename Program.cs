using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Wawelets
{
    static class Transforms
    {
        static Complex signal_in_point(int point, Complex[] z)
        {
            try
            {
                Complex result = -1;
                if (point >= 0 && point < z.Length)
                {
                    result = z[point];
                }
                else
                {
                    int period_num = point / z.Length;
                    if (point < 0)
                    {
                        period_num--;
                        point -= period_num * z.Length ;
                        result = z[point];
                    } else
                    {
                        point -= period_num * z.Length;
                        result = z[point];
                    }
                }
                return result;
            } catch (Exception e)
            {
                return -1;
            }
        }//Вычисление сигнала в точке
        static Complex[] IDFT(Complex[] fourier_z)//Обратное преобразование Фурье
        {
            try
            {
                int period = fourier_z.Length;
                Complex[] result = new Complex[period];
                Complex[][] fourier_base = new Complex[period][];
                for (int i = 0; i < period; i++)
                {
                    fourier_base[i] = new Complex[period];
                    result[i] = 0;
                }
                for (int i = 0; i < period; i++)
                {
                    for (int j = 0; j < period; j++)
                    {
                        float arg = (float)(2 * Math.PI * i * j / period);
                        fourier_base[i][j] = (Math.Cos(arg) + Complex.ImaginaryOne * Math.Sin(arg));
                        fourier_base[i][j] = Complex.Conjugate(fourier_base[i][j]);
                    }
                }
                for (int i = 0; i < period; i++)
                {
                    for (int j = 0; j < period; j++)
                    {
                        fourier_base[i][j] = Complex.Conjugate(fourier_base[i][j]) / period;
                    }
                }
                for (int i = 0; i < period; i++)
                {
                    for (int j = 0; j < period; j++)
                    {
                        result[i] += fourier_z[j] * fourier_base[i][j];
                    }
                }
                return result;
            } 
            catch (Exception e)
            {
                Complex[] err = new Complex[1];
                err[0] = -1;
                return err;
            }
        }
        static Complex[] DFT(Complex[] z)
        {
            try
            {
                int period = z.Length;
                Complex[] fourier_z = new Complex[period];
                Complex[][] fourier_base = new Complex[period][];
                for (int i = 0; i < period; i++)
                {
                    fourier_base[i] = new Complex[period];
                    fourier_z[i] = 0;
                }
                for (int i = 0; i < period; i++)
                {
                    for (int j = 0; j < period; j++)
                    {
                        float arg = (float)(2 * Math.PI * i * j / period);
                        fourier_base[i][j] = (Math.Cos(arg) + Complex.ImaginaryOne * Math.Sin(arg));
                        fourier_base[i][j] = Complex.Conjugate(fourier_base[i][j]);
                        fourier_z[i] += z[j] * fourier_base[i][j];
                    }
                }
                return fourier_z;
            }
            catch (Exception e)
            {
                Complex[] err = new Complex[1];
                err[0] = -1;
                return err;
            }
        }//Преобразование Фурье
        static Complex[] FFT(Complex[] z)
        {
            try
            {
                if (z.Length % 2 != 0) throw new Exception("Dim(z) % 2 != 0");
                int M = z.Length / 2;
                Complex[] fourier_z = new Complex[z.Length];
                for (int m = 0; m < M; m++)
                {
                    Complex U = Complex.Zero;
                    Complex V = Complex.Zero;
                    for (int n = 0; n < M; n++)
                    {
                        Complex Exp = new Complex(Math.Cos(-2 * Math.PI * m * n / M), Math.Sin(-2 * Math.PI * m * n / M));
                        U += z[2 * n] * Exp;
                        V += z[2 * n + 1] * Exp;
                    }
                    Complex Exp2 = new Complex(Math.Cos(-2 * Math.PI * m / z.Length), Math.Sin(-2 * Math.PI * m / z.Length));
                    fourier_z[m] = U + V * Exp2;
                    fourier_z[m + M] = U - V * Exp2;
                }
                return fourier_z;
            }
            catch (Exception e)
            {
                Complex[] err = new Complex[1];
                err[0] = -1;
                return err;
            }
        }//Быстрое преобразование Фурье
        static Complex[] IFFT(Complex[] fourier_z)
        {
            try
            {
                Complex Val;
                fourier_z = FFT(fourier_z);
                for (int i = 1; i <= fourier_z.Length / 2; i++)
                {
                    Val = fourier_z[i];
                    fourier_z[i] = fourier_z[fourier_z.Length - i] / (float)fourier_z.Length;
                    fourier_z[fourier_z.Length - i] = Val / (float)fourier_z.Length;
                }
                fourier_z[0] /= (float)fourier_z.Length;
                return fourier_z;
            }
            catch(Exception e)
            {
                Complex[] err = new Complex[1];
                err[0] = -1;
                return err;
            }
        }//Обратное быстрое преобразование Фурье
        static Complex[] Shift(int k, Complex[] vector)
        {
            try
            {
                Complex[] result = new Complex[vector.Length];
                result[result.Length] = vector[0];
                for (int i = 0; i - 1 < vector.Length; i++)
                {
                    result[i] = vector[i + 1];
                }
                return result;
            }
            catch (Exception e)
            {
                Complex[] err = new Complex[1];
                err[0] = -1;
                return err;
            }
        }//Оператор сдвига
        static Complex[] Convolution(Complex[] z, Complex[] b)
        {
            try
            {
                Complex[] result = new Complex[z.Length];
                for(int i = 0; i < result.Length; i++)
                {
                    result[i] = Complex.Zero;
                }
                for(int m = 0; m < z.Length; m++)
                {
                    for(int n = 0; n < z.Length; n++)
                    {
                        result[m] += signal_in_point(m - n, z) * b[n];
                    }
                }
                return result;
            }
            catch(Exception e)
            {
                Complex[] err = new Complex[1];
                err[0] = -1;
                return err;
            }
        }//Свертка
        static Complex[] DFT_Convolution(Complex[] z, Complex[] b)
        {
            try
            {
                z = DFT(z);
                b = DFT(b);
                Complex[] fourier_deconvolution = new Complex[z.Length];
                for(int i = 0; i < z.Length; i++)
                {
                    fourier_deconvolution[i] = z[i] * b[i];
                }
                fourier_deconvolution = IDFT(fourier_deconvolution);
                return fourier_deconvolution;
            }
            catch (Exception e)
            {
                Complex[] err = new Complex[1];
                err[0] = -1;
                return err;
            }
        }//Свертка через преобразования Фурье
        static Complex[] Conjugate_Reflection(Complex[] z)
        {
            try
            {
                Complex[] reflected_z = new Complex[z.Length];
                for(int i = 0; i < z.Length; i++)
                {
                    reflected_z[i] = Complex.Conjugate(z[z.Length - i]);
                }
                return reflected_z;
            }
            catch (Exception e)
            {
                Complex[] err = new Complex[1];
                err[0] = -1;
                return err;
            }
        }//Сопряженное отражение
        //static Complex[] Decomposition(Complex[] z, Complex[] w)
        //{
        //    try
        //    {
        //        w = Conjugate_Reflection(w);
        //        z = DFT_Convolution(z, w);
        //        return z;
        //    }
        //    catch (Exception e)
        //    {
        //        Complex[] err = new Complex[1];
        //        err[0] = -1;
        //        return err;
        //    }
        //}//Разложение по базису
        static Complex[] Half_Conjugate(Complex[] z)
        {
            if (z.Length % 2 != 0) throw new Exception("N % 2 != 0");
            try
            {
                for (int i = 0; i < z.Length; i++)
                {
                    z[i] = Math.Pow(-1, i) * z[i];
                }
                return z;
            }
            catch (Exception e)
            {
                Complex[] err = new Complex[1];
                err[0] = -1;
                return err;
            }
        }
        //static Complex[][] Wawelet_base(Complex[] U)
        //{

        //    try
        //    {
               
        //    }
        //    catch(Exception e)
        //    {

        //    } 
        //}
        static void Main(string[] args)
        {
            try
            {
                //Console.WriteLine("Enter period");
                //string str_period = Console.ReadLine();
                //Console.WriteLine("Enter point");
                //string str_point = Console.ReadLine();
                //int period = Convert.ToInt32(str_period);
                //int point = Convert.ToInt32(str_point);
                //Console.WriteLine("Sygnal's value is " + signal_in_point(period, point));

                Complex[] z = new Complex[3];
                Complex[] b = new Complex[3];
               // Complex[] fourier_z = new Complex[z.Length];
                z[0] = 1;
                z[1] = 1;
                z[2] = 1;
                //z[3] = 4;
                //Complex[] fourier_z = FFT(z);
                //Complex[] final_z = IFFT(fourier_z);
                //for (int i = 0; i < final_z.Length; i++)
                //{
                //    Console.WriteLine(fourier_z[i] + "             " + final_z[i]);
                //}
                b[0] = 2;
                b[1] = 3;
                b[2] = 0;
                Complex[] res = DFT_Convolution(z, b);
                for (int i = 0; i < res.Length; i++)
                {
                    Console.WriteLine(res[i]);
                }
                //FFT(z, fourier_z);
                //IFFT(fourier_z, z);
            }
            catch (Exception e)
            {
                Console.WriteLine("-1");
            }
        }
    }
}
