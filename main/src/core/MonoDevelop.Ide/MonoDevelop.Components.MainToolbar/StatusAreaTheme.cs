//
// StatusAreaTheme.cs
//
// Author:
//       Jason Smith <jason@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Diagnostics;
using Gtk;
using MonoDevelop.Components;
using Cairo;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Tasks;
using System.Collections.Generic;
using MonoDevelop.Ide.CodeCompletion;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui.Components;

using StockIcons = MonoDevelop.Ide.Gui.Stock;

namespace MonoDevelop.Components.MainToolbar
{
	class StatusAreaTheme
	{
		public void Render (Cairo.Context context, StatusArea.RenderArg arg)
		{
			DrawBackground (context, arg);

			if (arg.ErrorAnimationProgress > 0.001 && arg.ErrorAnimationProgress < .999) {
				DrawErrorAnimation (context, arg);
			}

			CairoExtensions.RoundedRectangle (context, arg.Allocation.X + 1.5, arg.Allocation.Y + 1.5, arg.Allocation.Width - 3, arg.Allocation.Height - 3, 3);
			context.LineWidth = 1;
			context.Color = Styles.StatusBarInnerColor;
			context.Stroke ();

			CairoExtensions.RoundedRectangle (context, arg.Allocation.X + 0.5, arg.Allocation.Y + 0.5, arg.Allocation.Width - 1, arg.Allocation.Height - 1, 3);
			context.LineWidth = 1;
			context.Color = Styles.StatusBarBorderColor;
			context.StrokePreserve ();

			if (arg.HoverProgress > 0.001f)
			{
				context.Clip ();
				int x1 = arg.Allocation.X + arg.MousePosition.X - 200;
				int x2 = x1 + 400;
				using (Cairo.LinearGradient gradient = new LinearGradient (x1, 0, x2, 0))
				{
					Cairo.Color targetColor = Styles.StatusBarFill1Color;
					Cairo.Color transparentColor = targetColor;
					targetColor.A = .7;
					transparentColor.A = 0;

					targetColor.A = .7 * arg.HoverProgress;

					gradient.AddColorStop (0.0, transparentColor);
					gradient.AddColorStop (0.5, targetColor);
					gradient.AddColorStop (1.0, transparentColor);

					context.Pattern = gradient;

					context.Rectangle (x1, arg.Allocation.Y, x2 - x1, arg.Allocation.Height);
					context.Fill ();
				}
				context.ResetClip ();
			} else {
				context.NewPath ();
			}

			int progress_bar_x = arg.ChildAllocation.X;
			int progress_bar_width = arg.ChildAllocation.Width;

			if (arg.CurrentPixbuf != null) {
				int y = arg.Allocation.Y + (arg.Allocation.Height - arg.CurrentPixbuf.Height) / 2;
				Gdk.CairoHelper.SetSourcePixbuf (context, arg.CurrentPixbuf, arg.ChildAllocation.X, y);
				context.Paint ();
				progress_bar_x += arg.CurrentPixbuf.Width + Styles.ProgressBarOuterPadding;
				progress_bar_width -= arg.CurrentPixbuf.Width + Styles.ProgressBarOuterPadding;
			}

			int center = arg.Allocation.Y + arg.Allocation.Height / 2;

			Gdk.Rectangle progressArea = new Gdk.Rectangle (progress_bar_x, center - Styles.ProgressBarHeight / 2, progress_bar_width, Styles.ProgressBarHeight);
			if (arg.ShowProgressBar || arg.ProgressBarAlpha > 0) {
				DrawProgressBar (context, arg.ProgressBarFraction, progressArea, arg);
				ClipProgressBar (context, progressArea);
			}

			int text_x = progress_bar_x + Styles.ProgressBarInnerPadding;
			int text_width = progress_bar_width - (Styles.ProgressBarInnerPadding * 2);

			float textTweenValue = arg.TextAnimationProgress;

			if (arg.LastText != null) {
				double opacity = 1.0f - textTweenValue;
				DrawString (arg.LastText, arg.LastTextIsMarkup, context, text_x, 
				            center - (int)(textTweenValue * arg.Allocation.Height * 0.3), text_width, opacity, arg.Pango, arg);
			}

			if (arg.CurrentText != null) {
				DrawString (arg.CurrentText, arg.CurrentTextIsMarkup, context, text_x, 
				            center + (int)((1.0f - textTweenValue) * arg.Allocation.Height * 0.3), text_width, textTweenValue, arg.Pango, arg);
			}

			if (arg.ShowProgressBar || arg.ProgressBarAlpha > 0)
				context.ResetClip ();
		}

		void DrawBackground (Cairo.Context context, StatusArea.RenderArg arg)
		{	
			CairoExtensions.RoundedRectangle (context, arg.Allocation.X + .5, arg.Allocation.Y + .5, 
			                                  arg.Allocation.Width - 1, arg.Allocation.Height - 1, 3);
			context.ClipPreserve ();

			using (LinearGradient lg = new LinearGradient (arg.Allocation.X, arg.Allocation.Y, arg.Allocation.X, arg.Allocation.Y + arg.Allocation.Height)) {
				lg.AddColorStop (0, Styles.StatusBarFill1Color);
				lg.AddColorStop (1, Styles.StatusBarFill4Color);

				context.Pattern = lg;
				context.FillPreserve ();
			}

			context.Save ();
			double midX = arg.Allocation.X + arg.Allocation.Width / 2.0;
			double midY = arg.Allocation.Y + arg.Allocation.Height;
			context.Translate (midX, midY);

			using (RadialGradient rg = new RadialGradient (0, 0, 0, 0, 0, arg.Allocation.Height * 1.2)) {
				rg.AddColorStop (0, Styles.StatusBarFill1Color);
				rg.AddColorStop (1, Styles.WithAlpha (Styles.StatusBarFill1Color, 0));

				context.Scale (arg.Allocation.Width / (double)arg.Allocation.Height, 1.0);
				context.Pattern = rg;
				context.Fill ();
			}
			context.Restore ();

			using (LinearGradient lg = new LinearGradient (0, arg.Allocation.Y, 0, arg.Allocation.Y + arg.Allocation.Height)) {
				lg.AddColorStop (0, Styles.StatusBarShadowColor1);
				lg.AddColorStop (1, Styles.WithAlpha (Styles.StatusBarShadowColor1, Styles.StatusBarShadowColor1.A * 0.2));

				CairoExtensions.RoundedRectangle (context, arg.Allocation.X + 0.5, arg.Allocation.Y + 1.5, arg.Allocation.Width - 1, arg.Allocation.Height - 3, 3);
				context.LineWidth = 1;
				context.Pattern = lg;
				context.Stroke ();
			}

			using (LinearGradient lg = new LinearGradient (0, arg.Allocation.Y, 0, arg.Allocation.Y + arg.Allocation.Height)) {
				lg.AddColorStop (0, Styles.StatusBarShadowColor2);
				lg.AddColorStop (1, Styles.WithAlpha (Styles.StatusBarShadowColor2, Styles.StatusBarShadowColor2.A * 0.2));

				CairoExtensions.RoundedRectangle (context, arg.Allocation.X + 0.5, arg.Allocation.Y + 2.5, arg.Allocation.Width - 1, arg.Allocation.Height - 5, 3);
				context.LineWidth = 1;
				context.Pattern = lg;
				context.Stroke ();
			}

			context.ResetClip ();
		}

		void DrawErrorAnimation (Cairo.Context context, StatusArea.RenderArg arg)
		{
			float opacity;
			int progress;

			if (arg.ErrorAnimationProgress < .5f) {
				progress = (int) (arg.ErrorAnimationProgress * arg.Allocation.Width * 2.4);
				opacity = 1.0f;
			} else {
				progress = (int) (arg.ErrorAnimationProgress * arg.Allocation.Width * 2.4);
				opacity = 1.0f - (arg.ErrorAnimationProgress - .5f) * 2;
			}

			CairoExtensions.RoundedRectangle (context, arg.Allocation.X + .5, arg.Allocation.Y + .5, 
			                                  arg.Allocation.Width - 1, arg.Allocation.Height - 1, 3);

			using (var lg = new LinearGradient (arg.Allocation.X - 2000 + progress, 0, arg.Allocation.X + progress, 0)) {
				lg.AddColorStop (0.00, Styles.WithAlpha (Styles.StatusBarErrorColor, 0.15 * opacity));
				lg.AddColorStop (0.85, Styles.WithAlpha (Styles.StatusBarErrorColor, 0.15 * opacity));
				lg.AddColorStop (0.98, Styles.WithAlpha (Styles.StatusBarErrorColor, 0.3 * opacity));
				lg.AddColorStop (1.00, Styles.WithAlpha (Styles.StatusBarErrorColor, 0.0 * opacity));

				context.Pattern = lg;
				context.Fill ();
			}
		}

		void DrawProgressBar (Cairo.Context context, double progress, Gdk.Rectangle bounding, StatusArea.RenderArg arg)
		{
			CairoExtensions.RoundedRectangle (context, bounding.X + 0.5, bounding.Y + 0.5, (bounding.Width - 1) * progress, bounding.Height - 1, 3);
			context.Clip ();

			CairoExtensions.RoundedRectangle (context, bounding.X + 0.5, bounding.Y + 0.5, bounding.Width - 1, bounding.Height - 1, 3);
			context.Color = Styles.WithAlpha (Styles.StatusBarProgressBackgroundColor, Styles.StatusBarProgressBackgroundColor.A * arg.ProgressBarAlpha);
			context.FillPreserve ();

			context.ResetClip ();

			context.Color = Styles.WithAlpha (Styles.StatusBarProgressOutlineColor, Styles.StatusBarProgressOutlineColor.A * arg.ProgressBarAlpha);
			context.LineWidth = 1;
			context.Stroke ();
		}

		void ClipProgressBar (Cairo.Context context, Gdk.Rectangle bounding)
		{
			CairoExtensions.RoundedRectangle (context, bounding.X + 0.5, bounding.Y + 0.5, bounding.Width - 1, bounding.Height - 1, 3);
			context.Clip ();
		}

		void DrawString (string text, bool isMarkup, Cairo.Context context, int x, int y, int width, double opacity, Pango.Context pango, StatusArea.RenderArg arg)
		{
			Pango.Layout pl = new Pango.Layout (pango);
			if (isMarkup)
				pl.SetMarkup (text);
			else
				pl.SetText (text);
			pl.FontDescription = Styles.StatusFont;
			pl.FontDescription.AbsoluteSize = Pango.Units.FromPixels (Styles.StatusFontPixelHeight);
			pl.Ellipsize = Pango.EllipsizeMode.End;
			pl.Width = Pango.Units.FromPixels(width);

			int w, h;
			pl.GetPixelSize (out w, out h);

			context.Save ();
			// use widget height instead of message box height as message box does not have a true height when no widgets are packed in it
			// also ensures animations work properly instead of getting clipped
			context.Rectangle (new Rectangle (x, arg.Allocation.Y, width, arg.Allocation.Height));
			context.Clip ();

			// Subtract off remainder instead of drop to prefer higher centering when centering an odd number of pixels
			context.MoveTo (x, y - h / 2 - (h % 2));

			Cairo.Color finalColor = Styles.StatusBarTextColor;
			finalColor.A = opacity;
			context.Color = finalColor;

			Pango.CairoHelper.ShowLayout (context, pl);
			pl.Dispose ();
			context.Restore ();
		}
	}
}
