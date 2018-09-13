using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfMath.Parsers;

namespace WpfMath.Example
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Choose file
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "SVG Files (*.svg)|*.svg|PNG Files (*.png)|*.png"
            };
            var result = saveFileDialog.ShowDialog();
            if (result == false) return;

            // Open stream
            var filename = saveFileDialog.FileName;
            switch (saveFileDialog.FilterIndex)
            {
                case 1:
                    var svgConv = new SVGConverter(formula.FormulaSettingsFile, formula.Formula, formula.Scale)
                    {
                        SystemTextFontName = formula.SystemTextFontName
                    };
                    svgConv.SaveGeometry(filename);
                    break;

                case 2:
                    using (var stream = new FileStream(filename, FileMode.Create))
                    {
                        TexFormulaParser formulaParser = new TexFormulaParser();
                        formulaParser.LoadSettings(formula.FormulaSettingsFile);
                        var texFormula = formulaParser.Parse(formula.Formula);
                        var renderer = texFormula.GetRenderer(TexStyle.Display, formula.Scale, formula.SystemTextFontName);

                        var bitmap = renderer.RenderToBitmap(0, 0);
                        var encoder = new PngBitmapEncoder
                        {
                            Frames = { BitmapFrame.Create(bitmap) }
                        };
                        encoder.Save(stream);
                    }
                    
                    break;

                default:
                    return;
            }
            
        }
        
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.formulaParser = new TexFormulaParser();

            var testFormula1 = "\\int_0^{\\infty}{x^{2n} e^{-a x^2} dx} = \\frac{2n-1}{2a} \\int_0^{\\infty}{x^{2(n-1)} e^{-a x^2} dx} = \\frac{(2n-1)!!}{2^{n+1}} \\sqrt{\\frac{\\pi}{a^{2n+1}}}";
            var testFormula2 = "\\int_a^b{f(x) dx} = (b - a) \\sum_{n = 1}^{\\infty}  {\\sum_{m = 1}^{2^n  - 1} { ( { - 1} )^{m + 1} } } 2^{ - n} f(a + m ( {b - a}  )2^{-n} )";
            var testFormula3 = @"L = \int_a^b \sqrt[4]{ \left| \sum_{i,j=1}^ng_{ij}\left(\gamma(t)\right) \left[\frac{d}{dt}x^i\circ\gamma(t) \right] \left{\frac{d}{dt}x^j\circ\gamma(t) \right} \right|}dt";
            
            //matrices
            var tf8 = @"\matrix{4&78&3 \\ 5 & 9  & 82 }";
            var tf9 = @"\bmatrix{4 & 78 & 3 \cr 5 & 9  & 22 }";
            var tf10 = @"\cases{x,&if x \ge 0;\cr -x,& otherwise.}";
            var tf11= @"\matrix{4&78&3\\57 & {\bmatrix{78 \\ 12}}  & 20782 }";
            var tf15 = @"R_2 = \left[ \amatrix{2 & 1 & 6 \\ 4 & 3 &14 \\ 2 & -2 & \alpha - 2}{0 \\ 4 \\ -\beta + 12} \right]";
            var tf17 = @"v \times w = \left( \matrix{v_2 w_3 - v_3 w_2 \\ v_3 w_1 - v_1 w_3 \\ v_1 w_2 - v_2 w_1} \right) where \medspace v= \left(\matrix{ v_1 \\ v_2 \\ v_3 }\right), w= \left( \matrix{w_1 \\ w_2  \\ w_3} \right)";
            this.inputTextBox.Text = tf17;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //
        }

        private void inputTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            formula.SelectionStart = inputTextBox.SelectionStart;
            formula.SelectionLength = inputTextBox.SelectionLength;
        }

    }
}
