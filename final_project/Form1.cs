using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace final_project
{
    public partial class Form1 : Form
    {

        private Stack<Bitmap> _undoStack;
        private Stack<Bitmap> _redoStack;
        public Form1()
        {
            InitializeComponent();

            // Set form size
            var primaryScreen = Screen.PrimaryScreen;
            var screenWidth = primaryScreen.Bounds.Width;
            var screenHeight = primaryScreen.Bounds.Height;
            this.Width = 1238;
            this.Height = 598;

            // Initialize bitmap and graphics objects
            bm = new Bitmap(pic.Width, pic.Height);
            g = Graphics.FromImage(bm);
            g.Clear(Color.White);
            pic.Image = bm;

            // Initialize undo and redo stacks
            _undoStack = new Stack<Bitmap>();
            _redoStack = new Stack<Bitmap>();

            // Set initial drawn state to false
            drawn = false;
        }

        Bitmap bm;
        Graphics g;
        bool paint = false;
        Point px, py;
        Pen p = new Pen(Color.Black, 1);
        Pen erase = new Pen(Color.White, 10);
        int index;
        int x, y, sX, sY, cX, cY;
        int Ax, Ay, Bx, By;
        bool drawn;
        bool drawback;
        ColorDialog cd = new ColorDialog();
        Color new_color;

        private void pic_MouseDown(object sender, MouseEventArgs e)
        {
            // If the user has drawn something previously, undo the last change
            if (drawn)
            {
                undo();
                drawn = false;
            }

            // Set paint state to true and save current location as "previous" location
            paint = true;
            py = e.Location;

            // Push the current bitmap onto the undo stack
            _undoStack.Push((Bitmap)bm.Clone());

            // Save the starting coordinates for a selection rectangle
            cX = e.X;
            cY = e.Y;

            // If the user has selected the "select" tool, save the starting coordinates
            if (index == 9)
            {
                Ax = e.X;
                Ay = e.Y;
            }
        }
        private void pic_MouseMove(object sender, MouseEventArgs e)
        {
            // If the mouse is being dragged, draw a line or erase
            if (paint)
            {
                if (index == 1)
                {
                    px = e.Location;
                    g.DrawLine(p, px, py);
                    py = px;
                }
                else if (index == 2)
                {
                    px = e.Location;
                    g.DrawLine(erase, px, py);
                    py = px;
                }
            }
            // Refresh the PictureBox
            pic.Refresh();
            // Update the current and previous coordinates
            x = e.X;
            y = e.Y;
            sX = e.X - cX;
            sY = e.Y - cY;
        }
        private void pic_MouseUp(object sender, MouseEventArgs e)
        {
            paint = false;

            // Calculate the width, height, and starting coordinates of the shape or selection rectangle
            sX = x - cX;
            sY = y - cY;
            Bx = x - Ax;
            By = y - Ay;

            // Draw the shape or selection rectangle based on the selected tool
            if (index == 3)
            {
                g.DrawEllipse(p, cX, cY, sX, sY);
            }
            else if (index == 4)
            {
                g.DrawRectangle(p, cX, cY, sX, sY);
            }
            else if (index == 5)
            {
                g.DrawLine(p, cX, cY, x, y);
            }
            else if (index == 9)
            {
                // Set the pen color and style for the selection rectangle
                var selectionPen = new Pen(Color.Black, 1);
                selectionPen.DashStyle = DashStyle.Dash;

                // Draw the selection rectangle on the paint surface
                g.DrawRectangle(selectionPen, cX, cY, sX, sY);
                Ax = cX;
                Ay = cY;
                Bx = sX;
                By = sY;
                drawn = true;
                pic.Refresh();
            }
        }

        private Rectangle GetRectangle(Point p1, Point p2)
        {
            var x = Math.Min(p1.X, p2.X);
            var y = Math.Min(p1.Y, p2.Y);
            var width = Math.Abs(p1.X - p2.X);
            var height = Math.Abs(p1.Y - p2.Y);
            return new Rectangle(x, y, width, height);
        }
        private void pic_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            if (paint)
            {
                if (index == 3)
                {
                    g.DrawEllipse(p, cX, cY, sX, sY);
                }
                else if (index == 4)
                {
                    g.DrawRectangle(p, cX, cY, sX, sY);
                }
                if (index == 5)
                {
                    g.DrawLine(p, cX, cY, x, y);
                }
                if (index == 9)
                {
                    // Set the pen color and style for the selection rectangle
                    var selectionPen = new Pen(Color.Black, 1);
                    selectionPen.DashStyle = DashStyle.Dash;

                    // Draw the selection rectangle on the paint surface
                    e.Graphics.DrawRectangle(selectionPen, cX, cY, sX, sY);
                }
            }
        }
        // This method takes a PictureBox and a Point as input parameters and returns a new Point object
        // The returned Point is calculated based on the ratio of the PictureBox image size and the PictureBox size
        // The X and Y values of the input Point are scaled accordingly
        static Point set_point(PictureBox pb, Point pt)
        {
            var pX = 1f * pb.Image.Width / pb.Width;
            var pY = 1f * pb.Image.Height / pb.Height;
            return new Point((int)(pt.X * pX), (int)(pt.Y * pY));
        }
        private void btn_color_Click(object sender, EventArgs e)
        {
            cd.ShowDialog();
            new_color = cd.Color;
            pic_color.BackColor = cd.Color;
            p.Color = cd.Color;
        }

        private void color_picker_MouseClick(object sender, MouseEventArgs e)
        {
            var point = set_point(color_picker, e.Location);
            pic_color.BackColor = ((Bitmap)color_picker.Image).GetPixel(point.X, point.Y);
            new_color = pic_color.BackColor;
            p.Color = pic_color.BackColor;
        }
        private void pic_MouseClick(object sender, MouseEventArgs e)
        {
            if (index == 7)
            {
                var point = set_point(pic, e.Location);
                fill(bm, point.X, point.Y, new_color);
            }
        }

        private void btn_pencil_Click(object sender, EventArgs e)
        {
            index = 1;
        }
        private void btn_eraser_Click(object sender, EventArgs e)
        {
            index = 2;
        }
        private void btn_elipse_Click(object sender, EventArgs e)
        {
            index = 3;
        }
        private void btn_rect_Click(object sender, EventArgs e)
        {
            index = 4;
        }
        private void btn_line_Click(object sender, EventArgs e)
        {
            index = 5;
        }
        private void btn_fill_Click(object sender, EventArgs e)
        {
            index = 7;
        }
        private void clear()
        {
            bm = new Bitmap(pic.Width, pic.Height);
            g = Graphics.FromImage(bm);
            g.Clear(Color.White);
            pic.Image = bm;
        }
        private void save()
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Image (.svg)|.svg|(.png)|.png|All Files (.)|.";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var btm = bm.Clone(new Rectangle(0, 0, pic.Width, pic.Height), bm.PixelFormat);
                btm.Save(sfd.FileName, ImageFormat.Jpeg);
            }
        }

        private void pic_control_Click(object sender, EventArgs e)
        {

        }

        private void open()
        {
            var Op = new OpenFileDialog();
            var dr = Op.ShowDialog();
            if (dr == DialogResult.OK)
            {
                var bitmap = (Bitmap)Image.FromFile(Op.FileName);
                pic.Image = bitmap;
                g = Graphics.FromImage(bitmap);
                pic.Refresh();
            }
        }
        private void undo()
        {
            if (drawback)
            {
                drawn = true; drawback = false;
                var selectionPen = new Pen(Color.Black, 1);
                undo();
            }
            if (drawn)
                drawn = false;
            if (_undoStack.Count > 0)
            {
                _redoStack.Push(bm);
                pic.Image = null;
                bm = _undoStack.Pop();
                g = Graphics.FromImage(bm);
                pic.Image = bm;
            }
            else
            {
                bm = new Bitmap(pic.Width, pic.Height);
                g = Graphics.FromImage(bm);
                g.Clear(Color.White);
                pic.Image = bm;
            }
            pic.Refresh();
        }
        private void remove()
        {
            var brush = new SolidBrush(Color.White);
            var rect = new Rectangle(Ax, Ay, Bx + 1, By + 1);
            g.FillRectangle(brush, rect);
            _undoStack.Push((Bitmap)bm.Clone());
            drawback = true;
            drawn = false;
            pic.Refresh();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clear();
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save();
        }
        private void openOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            open();
        }
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            undo();
        }


        private void validate(Bitmap bm, Stack<Point> sp, int x, int y, Color old_color, Color new_color)
        {
            var cx = bm.GetPixel(x, y);
            if (cx == old_color)
            {
                sp.Push(new Point(x, y));
                bm.SetPixel(x, y, new_color);
            }
        }

        public void fill(Bitmap bm, int x, int y, Color new_clr)
        {
            var old_color = bm.GetPixel(x, y);
            var pixel = new Stack<Point>();
            pixel.Push(new Point(x, y));
            bm.SetPixel(x, y, new_clr);
            if (old_color == new_clr) return;

            while (pixel.Count > 0)
            {
                var pt = (Point)pixel.Pop();
                if (pt.X > 0 && pt.Y > 0 && pt.X < bm.Width - 1 && pt.Y < bm.Height - 1)
                {
                    validate(bm, pixel, pt.X - 1, pt.Y, old_color, new_clr);
                    validate(bm, pixel, pt.X, pt.Y - 1, old_color, new_clr);
                    validate(bm, pixel, pt.X + 1, pt.Y, old_color, new_clr);
                    validate(bm, pixel, pt.X, pt.Y + 1, old_color, new_clr);
                }
            }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.S:
                    save();
                    break;
                case Keys.O:
                    open();
                    break;
                case Keys.C:
                    clear();
                    break;
                case Keys.U:
                    undo();
                    break;
                case Keys.D:
                    remove();
                    break;
            }
        }
        private void btn_select_Click(object sender, EventArgs e)
        {
            index = 9;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
        }

    }
}
