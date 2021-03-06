using System;
using WpfMath.Atoms;
using WpfMath.Boxes;

namespace WpfMath
{
    // Atom representing radical (nth-root) construction.
    internal class Radical : Atom
    {
        private const string sqrtSymbol = "sqrt";

        private const double scale = 0.55;

        public Radical(SourceSpan source, Atom baseAtom, Atom degreeAtom = null,bool degreespecified=false )
            : base(source)
        {
            this.BaseAtom = baseAtom;
            this.DegreeAtom = degreeAtom;
            this.DegreeSpecified=degreespecified;
        }

        public Atom BaseAtom { get; }

        public Atom DegreeAtom { get; }
        /// <summary>
        /// Gets a value that specifies whether a degree was requested.
        /// </summary>
        public bool DegreeSpecified{get;}
        protected override Box CreateBoxCore(TexEnvironment environment)
        {
            var texFont = environment.MathFont;
            var style = environment.Style;

            // Calculate minimum clearance amount.
            double clearance;
            var defaultRuleThickness = texFont.GetDefaultLineThickness(style);
            if (style < TexStyle.Text)
                clearance = texFont.GetXHeight(style, texFont.GetCharInfo(sqrtSymbol, style).Value.FontId);
            else
                clearance = defaultRuleThickness;
            clearance = defaultRuleThickness + Math.Abs(clearance) / 4;

            // Create box for base atom, in cramped style.
            var baseBox =this.BaseAtom== null? StrutBox.Empty: this.BaseAtom.CreateBox(environment.GetCrampedStyle());

            // Create box for radical sign.
            var totalHeight = baseBox.Height + baseBox.Depth;
            var radicalSignBox = DelimiterFactory.CreateBox(sqrtSymbol, totalHeight + clearance + defaultRuleThickness, environment, Source);
            radicalSignBox.Source = Source;

            // Add half of excess height to clearance.
            var delta = radicalSignBox.Depth - (totalHeight + clearance);
            clearance += delta / 2;

            // Create box for root base containing base box.
            var overBar = new OverBar(environment, baseBox, clearance, radicalSignBox.Height)
            {
                Shift = -defaultRuleThickness,
            };

            //Create box to hold the radical and the degree atom(if it exists)
            var radicalContainerBox = new VerticalBox();
            //Create a box for creating a space for the radical sign box's left shift and width
            var horizoverlapbox= new StrutBox(0,0,0,0);
            radicalContainerBox.Add(horizoverlapbox);
            radicalContainerBox.Add(radicalSignBox);

            // Create box for root atom.
            var radrootBox = this.DegreeAtom == null ?(this.DegreeSpecified? StrutBox.Empty:NullAtom.NullBox) : this.DegreeAtom.CreateBox(environment.GetRootStyle());
            var bottomShift = scale * (radicalSignBox.Height + radicalSignBox.Depth);
            var rcbItemsdiff = radicalSignBox.TotalHeight - radrootBox.TotalHeight;
            bottomShift = rcbItemsdiff;
            radrootBox.Shift =0;
            if (radrootBox.TotalHeight< radicalSignBox.TotalHeight / 2)
            {
                bottomShift = (radicalSignBox.TotalHeight/2) + radrootBox.TotalHeight;
            }
            var Vnegspace = new StrutBox(0, -bottomShift, 0, 0);
            radicalContainerBox.Add(Vnegspace);
            radicalContainerBox.Add(radrootBox);

            var leftshift = this.DegreeAtom== null? 0: radrootBox.TotalWidth - radicalSignBox.TotalWidth / 2;
            if(radrootBox.TotalWidth<(radicalSignBox.TotalWidth/2))
                leftshift=0;
            radicalSignBox.Shift= leftshift;
            //increase the left overlap width
            horizoverlapbox.Width= leftshift+radicalSignBox.Width;

            // Create result box.
            var resultBox = new HorizontalBox();

            resultBox.Add(radicalContainerBox);
            //adjust the vertical shift of the radicalContainerBox
            radicalContainerBox.Shift = -defaultRuleThickness- radicalContainerBox.Depth;
            var leftpad = radicalContainerBox.TotalWidth -radicalSignBox.Shift - radicalSignBox.TotalWidth;

            resultBox.Add(new StrutBox(leftpad, 0, 0, 0));
            overBar.Shift = -defaultRuleThickness - radicalContainerBox.Depth;
            resultBox.Add(overBar);

            return resultBox;
        }
    }
}
