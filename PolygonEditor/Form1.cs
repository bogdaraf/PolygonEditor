using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PolygonEditor
{
    /// <summary>
    /// This class contains the logic of the application
    /// </summary>
    public partial class Form1 : Form {
        int state = 0;
        int currPolygon = -1;

        List<List<Point>> vertices = new List<List<Point>>();
        List<Relation> relations = new List<Relation>();

        bool mouseIsDown = false;
        MyPoint vertexBeingMoved = null;
        Edge edgeBeingMoved = null;
        MyPoint differenceA = null;
        MyPoint differenceB = null;
        MyPoint vertexPointed = null;
        Edge edgePointed = null;
        int polygonBeingMoved = -1;
        List<Point> differences = null;
        Relation tempRelation;
        bool creatingRelation = false;
        int equalCounter = 1;
        int parallelCounter = 1;

        int thickness = 1;


        /// <summary>
        /// Constructor initializes the Form component
        /// </summary>
        public Form1() {
            InitializeComponent();
        }


        /// <summary>
        /// This function is called every time the form needs to be refreshed
        /// Vertices, edges and other components are drawn on the screen
        /// </summary>
        private void Form1_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;

            for (int i = 0; i < vertices.Count; i++) {
                Pen pen = new Pen(Color.Black, 2);

                //draw edges
                for (int j = 0; j < vertices[i].Count - 1; j++) {
                    Brush brush = new SolidBrush(Color.Black);
                    drawLine(g, brush, vertices[i][j].X, vertices[i][j].Y, vertices[i][j + 1].X, vertices[i][j + 1].Y);
                }
                if((state != 1 && vertices[i].Count - 1 > 0) || (state == 1 && currPolygon != i)) {
                    Brush brush = new SolidBrush(Color.Black);
                    drawLine(g, brush, vertices[i][vertices[i].Count - 1].X, vertices[i][vertices[i].Count - 1].Y, vertices[i][0].X, vertices[i][0].Y);
                }

                //draw vertices
                for (int j = 0; j < vertices[i].Count; j++) {
                    Brush brush = new SolidBrush(Color.Black);
                    float radius = 4;
                    g.FillEllipse(brush, vertices[i][j].X - radius, vertices[i][j].Y - radius, 2 * radius, 2 * radius);
                }

            }

            //draw icons of relations
            for(int i=0; i<relations.Count; i++) {
                String drawString;

                if(relations[i].type == 6) {
                    drawString = "E" + relations[i].number.ToString();
                }
                else {
                    drawString = "P" + relations[i].number.ToString();
                }
                
                Font drawFont = new Font("Arial", 16);
                Brush brush = new SolidBrush(Color.Red);

                int x1 = (vertices[relations[i].M.A.X][relations[i].M.A.Y].X + vertices[relations[i].M.B.X][relations[i].M.B.Y].X) / 2 + 2;
                int y1 = (vertices[relations[i].M.A.X][relations[i].M.A.Y].Y + vertices[relations[i].M.B.X][relations[i].M.B.Y].Y) / 2 + 2;
                int x2 = (vertices[relations[i].N.A.X][relations[i].N.A.Y].X + vertices[relations[i].N.B.X][relations[i].N.B.Y].X) / 2 + 2;
                int y2 = (vertices[relations[i].N.A.X][relations[i].N.A.Y].Y + vertices[relations[i].N.B.X][relations[i].N.B.Y].Y) / 2 + 2;

                g.DrawString(drawString, drawFont, brush, x1, y1);
                g.DrawString(drawString, drawFont, brush, x2, y2);
            }

            if(state == 0) {

                //display a pointed vertex in a different color
                if(vertexPointed != null) {
                    Brush brush = new SolidBrush(Color.Red);
                    float radius = 5;
                    g.FillEllipse(brush, vertices[vertexPointed.X][vertexPointed.Y].X - radius, vertices[vertexPointed.X][vertexPointed.Y].Y - radius, 2 * radius, 2 * radius);
                }
            }

            if(state == 0 || state == 4) {

                //display a pointed edge in a different color
                if(edgePointed != null) {
                    Pen pen = new Pen(Color.Red, 2);
                    g.DrawLine(pen, vertices[edgePointed.A.X][edgePointed.A.Y], vertices[edgePointed.B.X][edgePointed.B.Y]);
                }
            }

            //create new polygon - draw a temporary vertex and edge
            if(state == 1) {
                int cursorX = PointToClient(Cursor.Position).X;
                int cursorY = PointToClient(Cursor.Position).Y;

                if(currPolygon >= 0 && vertices[currPolygon].Count > 0) {
                    //Pen pen = new Pen(Color.Black, 2);
                    //g.DrawLine(pen, list[currPolygon].Last(), new Point(cursorX, cursorY));
                    Brush brush1 = new SolidBrush(Color.Black);
                    drawLine(g, brush1, vertices[currPolygon].Last().X, vertices[currPolygon].Last().Y, cursorX, cursorY);
                }

                Brush brush = new SolidBrush(Color.Red);
                float radius = 5;
                g.FillEllipse(brush, cursorX - radius, cursorY - radius, 2 * radius, 2 * radius);
            }
        }


        /// <summary>
        /// Draws a line between two points using Bresenham's algorithm
        /// </summary>
        /// <param name="g"> Graphics context</param>
        /// <param name="brush"> A brush used to draw the line</param>
        /// <param name="x0"> X coordinate of the first point of the line</param>
        /// <param name="y0"> Y coordinate of the first point of the line</param>
        /// <param name="x1"> X coordinate of the last point of the line</param>
        /// <param name="y1"> Y coordinate of the last point of the line</param>
        private void drawLine(Graphics g, Brush brush, int x0, int y0, int x1, int y1) {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep) {
                int t;
                t = x0; // swap x0 and y0
                x0 = y0;
                y0 = t;
                t = x1; // swap x1 and y1
                x1 = y1;
                y1 = t;
            }
            if (x0 > x1) {
                int t;
                t = x0; // swap x0 and x1
                x0 = x1;
                x1 = t;
                t = y0; // swap y0 and y1
                y0 = y1;
                y1 = t;
            }
            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;
            for (int x = x0; x <= x1; x++) {
                if(steep) {
                    for(int j = y + thickness / 2; j >= y - (thickness - 1) / 2; j--) {
                        g.FillRectangle(brush, j, x, 1, 1); // used to change color of one pixel
                    }
                }
                else {
                    for (int j = x + thickness / 2; j >= x - (thickness - 1) / 2; j--) {
                        g.FillRectangle(brush, j, y, 1, 1); // used to change color of one pixel
                    }
                }

                error = error - dy;
                if (error < 0) {
                    y += ystep;
                    error += dx;
                }
            }
        }


        /// <summary>
        /// This function is called every time the form is clicked
        /// </summary>
        private void Form1_Click(object sender, EventArgs e) {
            int cursorX = PointToClient(Cursor.Position).X;
            int cursorY = PointToClient(Cursor.Position).Y;

            if(state == 1) {
                //if new vertex is close enough to the first vertex then finish the polygon creation process; min 3 vertices
                if(vertices[currPolygon].Count > 0) {
                    double distanceSq = Math.Pow(vertices[currPolygon].First().X - cursorX, 2) + Math.Pow(vertices[currPolygon].First().Y - cursorY, 2);

                    if(distanceSq < 5 * 5 && vertices[currPolygon].Count >= 3) {
                        state = 0;
                        return;
                    }
                }

                //two vertices can't be too close to each other
                for (int i = 0; i < vertices[currPolygon].Count; i++) {
                    double distanceSq = Math.Pow(vertices[currPolygon][i].X - cursorX, 2) + Math.Pow(vertices[currPolygon][i].Y - cursorY, 2);
                    if(distanceSq < 4 * 4) {
                        return;
                    }
                }

                vertices[currPolygon].Add(new Point(cursorX, cursorY));
            }

            //remove a polygon
            if(state == 2) {
                for (int i = 0; i < vertices.Count; i++) {
                    for (int j = 0; j < vertices[i].Count; j++) {
                        double distanceSq = Math.Pow(vertices[i][j].X - cursorX, 2) + Math.Pow(vertices[i][j].Y - cursorY, 2);
                        if(distanceSq < 4 * 4) {
                            vertices.RemoveAt(i);
                            currPolygon--;
                            state = 0;

                            for(int k=0; k<relations.Count; k++) {
                                if(relations[k].M.A.X == i) {
                                    relations.RemoveAt(k);
                                    k--;
                                }
                            }

                            Refresh();
                            return;
                        }
                    }
                }
            }

            //remove a vertex
            if(state == 3) {
                for (int i = 0; i < vertices.Count; i++) {
                    for (int j = 0; j < vertices[i].Count; j++) {
                        double distanceSq = Math.Pow(vertices[i][j].X - cursorX, 2) + Math.Pow(vertices[i][j].Y - cursorY, 2);
                        if(distanceSq < 4 * 4 && vertices[i].Count > 3) {
                            Edge edge1 = null;
                            Edge edge2 = null;

                            if(j != 0) {
                                edge1 = new Edge(new Point(i, j - 1), new Point(i, j));
                            }
                            else {
                                edge1 = new Edge(new Point(i, vertices[i].Count - 1), new Point(i, j));
                            }
                            if(j != vertices[i].Count - 1) {
                                edge2 = new Edge(new Point(i, j), new Point(i, j + 1));
                            }
                            else {
                                edge2 = new Edge(new Point(i, j), new Point(i, 0));
                            }

                            if(getRelation(edge1) != null) {
                                relations.Remove(getRelation(edge1));
                            }
                            if (getRelation(edge2) != null) {
                                relations.Remove(getRelation(edge2));
                            }

                            for(int k=0; k<relations.Count; k++) {
                                if(relations[k].M.B.Y > j) {
                                    relations[k].M.A = new Point(relations[k].M.A.X, relations[k].M.A.Y - 1);
                                    relations[k].M.B = new Point(relations[k].M.B.X, relations[k].M.B.Y - 1);
                                }
                                if (relations[k].N.B.Y > j) {
                                    relations[k].N.A = new Point(relations[k].N.A.X, relations[k].N.A.Y - 1);
                                    relations[k].N.B = new Point(relations[k].N.B.X, relations[k].N.B.Y - 1);
                                }
                            }

                            vertices[i].RemoveAt(j);
                            state = 0;

                            Refresh();
                            return;
                        }
                    }
                }
            }

            //add new vertex in the middle of an edge
            if(state == 4 && edgePointed != null) {
                if (getRelation(edgePointed) != null) {
                    relations.Remove(getRelation(edgePointed));
                }

                Point A = edgePointed.A;
                Point B = edgePointed.B;

                Point newVertexCoords = new Point((vertices[A.X][A.Y].X + vertices[B.X][B.Y].X) / 2, (vertices[A.X][A.Y].Y + vertices[B.X][B.Y].Y) / 2);
                vertices[B.X].Insert(B.Y, newVertexCoords);

                for (int k = 0; k < relations.Count; k++) {
                    if (relations[k].M.B.Y > B.Y) {
                        relations[k].M.A = new Point(relations[k].M.A.X, relations[k].M.A.Y + 1);
                        relations[k].M.B = new Point(relations[k].M.B.X, relations[k].M.B.Y + 1);
                    }
                    if (relations[k].N.B.Y > B.Y) {
                        relations[k].N.A = new Point(relations[k].N.A.X, relations[k].N.A.Y + 1);
                        relations[k].N.B = new Point(relations[k].N.B.X, relations[k].N.B.Y + 1);
                    }
                }

                state = 0;

                Refresh();
                return;
            }

            //add a relation
            if (state == 6 || state == 7) {
                if (edgePointed != null && getRelation(edgePointed) == null && creatingRelation == false) {
                    tempRelation = new Relation();
                    tempRelation.M = edgePointed;
                    tempRelation.type = state;
                    creatingRelation = true;
                }

                else if (edgePointed != null && getRelation(edgePointed) == null && creatingRelation == true
                        && !(edgePointed.A.X == tempRelation.M.A.X && edgePointed.A.Y == tempRelation.M.A.Y && edgePointed.B.X == tempRelation.M.B.X && edgePointed.B.Y == tempRelation.M.B.Y)) {
                    tempRelation.N = edgePointed;

                    if (tempRelation.type == 6) {
                        tempRelation.number = equalCounter++;
                    }
                    else {
                        tempRelation.number = parallelCounter++;
                    }

                    relations.Add(tempRelation);

                    updatePolygon(new MyPoint(tempRelation.M.A.X, 0), null);

                    tempRelation = null;
                    creatingRelation = false;
                    
                    state = 0;
                }
            }

            //remove a relation
            if(state == 8 && edgePointed != null && getRelation(edgePointed) != null) {
                relations.Remove(getRelation(edgePointed));

                state = 0;
            }
        }


        /// <summary>
        /// This function is called every time the mouse button is released
        /// </summary>
        private void Form1_MouseUp(object sender, MouseEventArgs e) {
            if(state == 0 || state == 5) {
                vertexBeingMoved = null;
                edgeBeingMoved = null;
                mouseIsDown = false;

                if(polygonBeingMoved != -1) {
                    polygonBeingMoved = -1;
                    state = 0;
                }
            }
        }


        /// <summary>
        /// This function is called every time the mouse button is pressed
        /// </summary>
        private void Form1_MouseDown(object sender, MouseEventArgs e) {
            int cursorX = PointToClient(Cursor.Position).X;
            int cursorY = PointToClient(Cursor.Position).Y;

            //set which vertex will be moved
            if(state == 0 && vertexPointed != null) {
                
                vertexBeingMoved = vertexPointed;
            }

            //set which edge will be moved
            if(state == 0 && edgePointed != null) {
                edgeBeingMoved = edgePointed;
                differenceA = new MyPoint(vertices[edgeBeingMoved.A.X][edgeBeingMoved.A.Y].X - cursorX, vertices[edgeBeingMoved.A.X][edgeBeingMoved.A.Y].Y - cursorY);
                differenceB = new MyPoint(vertices[edgeBeingMoved.B.X][edgeBeingMoved.B.Y].X - cursorX, vertices[edgeBeingMoved.B.X][edgeBeingMoved.B.Y].Y - cursorY);
            }

            //set which polygon will be moved
            if(state == 5) {
                if(vertexPointed != null) {
                    polygonBeingMoved = vertexPointed.X;
                }
                if(edgePointed != null) {
                    polygonBeingMoved = edgePointed.A.X;
                }
                if(vertexPointed != null || edgePointed != null) {
                    differences = new List<Point>();
                    for (int i = 0; i < vertices[polygonBeingMoved].Count; i++) {
                        differences.Add(new Point(vertices[polygonBeingMoved][i].X - cursorX, vertices[polygonBeingMoved][i].Y - cursorY));
                    }
                }
            }

            mouseIsDown = true;
        }


        /// <summary>
        /// This function is called every time the mouse is moved
        /// </summary>
        private void Form1_MouseMove(object sender, MouseEventArgs e) {
            int cursorX = PointToClient(Cursor.Position).X;
            int cursorY = PointToClient(Cursor.Position).Y;

            if(state == 0 || state >= 4) {
                vertexPointed = null;

                //check if cursor is pointing a vertex
                for (int i = 0; i < vertices.Count; i++) {
                    for (int j = 0; j < vertices[i].Count; j++) {
                        double distanceSq = Math.Pow(vertices[i][j].X - cursorX, 2) + Math.Pow(vertices[i][j].Y - cursorY, 2);
                        if(distanceSq < 4 * 4) {
                            vertexPointed = new MyPoint(i, j);
                        }
                    }
                }

                edgePointed = null;

                //check if cursor is pointing an edge
                if(vertexPointed == null) {
                    for (int i = 0; i < vertices.Count; i++) {
                        for (int j = 0; j < vertices[i].Count; j++) {
                            if(PointLineDist(new Point(cursorX, cursorY), vertices[i][j], vertices[i][(j + 1) % vertices[i].Count]) < 4) {
                                bool inProperAreaX = false;
                                bool inProperAreaY = false;
                                if(vertices[i][j].X <= vertices[i][(j + 1) % vertices[i].Count].X && cursorX >= vertices[i][j].X && cursorX <= vertices[i][(j + 1) % vertices[i].Count].X) {
                                    inProperAreaX = true;
                                }
                                else if(vertices[i][j].X >= vertices[i][(j + 1) % vertices[i].Count].X && cursorX <= vertices[i][j].X && cursorX >= vertices[i][(j + 1) % vertices[i].Count].X) {
                                    inProperAreaX = true;
                                }
                                if(vertices[i][j].Y <= vertices[i][(j + 1) % vertices[i].Count].Y && cursorY >= vertices[i][j].Y && cursorY <= vertices[i][(j + 1) % vertices[i].Count].Y) {
                                    inProperAreaY = true;
                                }
                                else if(vertices[i][j].Y >= vertices[i][(j + 1) % vertices[i].Count].Y && cursorY <= vertices[i][j].Y && cursorY >= vertices[i][(j + 1) % vertices[i].Count].Y) {
                                    inProperAreaY = true;
                                }

                                if(inProperAreaX && inProperAreaY) {
                                    edgePointed = new Edge(new Point(i, j), new Point(i, (j + 1) % vertices[i].Count));
                                }
                            }
                        }
                    }
                }
            }

            if(state == 0) {

                //move a vertex + update the polygon with regard to relations
                if(mouseIsDown && vertexBeingMoved != null) {
                    vertices[vertexBeingMoved.X][vertexBeingMoved.Y] = new Point(cursorX, cursorY);

                    updatePolygon(vertexBeingMoved, null);
                }

                //move an edge
                if(mouseIsDown && edgeBeingMoved != null) {
                    Point A = edgeBeingMoved.A;
                    Point B = edgeBeingMoved.B;

                    vertices[A.X][A.Y] = new Point(cursorX + differenceA.X, cursorY + differenceA.Y);
                    vertices[B.X][B.Y] = new Point(cursorX + differenceB.X, cursorY + differenceB.Y);

                    updatePolygon(null, edgeBeingMoved);
                }
            }

            if(state == 5) {

                //move a polygon
                if(mouseIsDown && polygonBeingMoved != -1) {
                    for(int i=0; i<differences.Count; i++) {
                        vertices[polygonBeingMoved][i] = new Point(cursorX + differences[i].X, cursorY + differences[i].Y);
                    }
                }
            }

            Refresh();
        }


        /// <summary>
        /// This function is called when the "Create new polygon" button is clicked
        /// </summary>
        private void button1_Click(object sender, EventArgs e) {
            state = 1;
            currPolygon++;
            vertices.Add(new List<Point>());

            Refresh();
        }


        /// <summary>
        /// This function is called when the "Remove a polygon" button is clicked
        /// </summary>
        private void button2_Click(object sender, EventArgs e) {
            state = 2;
        }


        /// <summary>
        /// This function is called when the "Remove a vertex" button is clicked
        /// </summary>
        private void button3_Click(object sender, EventArgs e) {
            state = 3;
        }


        /// <summary>
        /// This function is called when the "Add new vertex in the middle of an edge" button is clicked
        /// </summary>
        private void button4_Click(object sender, EventArgs e) {
            state = 4;
        }


        /// <summary>
        /// This function is called when the "Move a polygon" button is clicked
        /// </summary>
        private void button5_Click(object sender, EventArgs e) {
            state = 5;
        }


        /// <summary>
        /// This function is called when the "Add a relation - equal edges" button is clicked
        /// </summary>
        private void button6_Click(object sender, EventArgs e) {
            state = 6;
        }

        /// <summary>
        /// This function is called when the "Add a relation - parallel edges" button is clicked
        /// </summary>
        private void button7_Click(object sender, EventArgs e) {
            state = 7;
        }


        /// <summary>
        /// This function is called when the "Remove a relation" button is clicked
        /// </summary>
        private void button8_Click(object sender, EventArgs e) {
            state = 8;
        }


        /// <summary>
        /// This function is called when the "Reset to state 0" button is clicked
        /// </summary>
        private void button9_Click(object sender, EventArgs e) {
            state = 0;
            mouseIsDown = false;
            tempRelation = null;
            creatingRelation = false;
        }


        /// <summary>
        /// This function is called when the "Increase thickness" button is clicked
        /// </summary>
        private void button11_Click(object sender, EventArgs e) {
            thickness++;
        }


        /// <summary>
        /// This function is called when the "Decrease thickness" button is clicked
        /// </summary>
        private void button12_Click(object sender, EventArgs e) {
            if(thickness > 1) {
                thickness--;
            }
        }


        /// <summary>
        /// This function is called when the "Export to json" button is clicked
        /// The data about polygons and their locations is exported into a json format
        /// </summary>
        private void button13_Click(object sender, EventArgs e) {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw)) {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                writer.WritePropertyName("polygons");
                writer.WriteStartArray();

                for (int i = 0; i < vertices.Count; i++) {
                    writer.WriteStartObject();
                    writer.WritePropertyName("points");
                    writer.WriteStartArray();

                    for (int j = 0; j < vertices[i].Count; j++) {
                        writer.WriteStartObject();
                        writer.WritePropertyName("x");
                        writer.WriteValue(vertices[i][j].X);
                        writer.WritePropertyName("y");
                        writer.WriteValue(vertices[i][j].Y);
                        writer.WriteEndObject();
                    }

                    writer.WriteEndArray();
                    writer.WritePropertyName("relations");
                    writer.WriteStartArray();

                    for (int j = 0; j < relations.Count; j++) {
                        if (relations[j].M.A.X == i) {
                            writer.WriteStartObject();
                            writer.WritePropertyName("e1");

                            int k = relations[j].M.A.Y > relations[j].M.B.Y ? relations[j].M.A.Y : relations[j].M.B.Y;
                            if (relations[j].M.A.Y == 0 && relations[j].M.B.Y == vertices[i].Count - 1) {
                                k = vertices[i].Count;
                            }
                            if (relations[j].M.B.Y == 0 && relations[j].M.A.Y == vertices[i].Count - 1) {
                                k = vertices[i].Count;
                            }

                            writer.WriteValue(k);
                            writer.WritePropertyName("e2");

                            k = relations[j].N.A.Y > relations[j].N.B.Y ? relations[j].N.A.Y : relations[j].N.B.Y;
                            if (relations[j].N.A.Y == 0 && relations[j].N.B.Y == vertices[i].Count - 1) {
                                k = vertices[i].Count;
                            }
                            if (relations[j].N.B.Y == 0 && relations[j].N.A.Y == vertices[i].Count - 1) {
                                k = vertices[i].Count;
                            }

                            writer.WriteValue(k);
                            writer.WritePropertyName("type");
                            writer.WriteValue(relations[j].type == 6 ? 0 : 1);
                            writer.WriteEndObject();
                        }
                    }

                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
                //writer.WriteEnd();
                writer.WriteEndObject();

                MessageBox.Show(sb.ToString());
            }
        }


        /// <summary>
        /// This function is called when the "Add a predefined polygon" button is clicked
        /// </summary>
        private void button10_Click(object sender, EventArgs e) {
            currPolygon++;

            vertices.Add(new List<Point>());
            vertices[currPolygon].Add(new Point(380, 180));
            vertices[currPolygon].Add(new Point(530, 210));
            vertices[currPolygon].Add(new Point(510, 360));
            vertices[currPolygon].Add(new Point(360, 370));
            vertices[currPolygon].Add(new Point(260, 280));

            Relation relation1 = new Relation(new Edge(new Point(currPolygon, 1), new Point(currPolygon, 2)), new Edge(new Point(currPolygon, 3), new Point(currPolygon, 4)), 6);
            relation1.number = equalCounter++;
            relations.Add(relation1);
            Relation relation2 = new Relation(new Edge(new Point(currPolygon, 0), new Point(currPolygon, 1)), new Edge(new Point(currPolygon, 2), new Point(currPolygon, 3)), 7);
            relation2.number = parallelCounter++;
            relations.Add(relation2);

            updatePolygon(new MyPoint(0, 0), null);
            Refresh();
        }


        /// <summary>
        /// This function calculates the distance between point p and a line going through points p1 and p2
        /// </summary>
        /// <param name="p"> Coordinates of the point p</param>
        /// <param name="p1"> Coordinates of the pint p1</param>
        /// <param name="p2"> Coordinates of the point p2</param>
        private double PointLineDist(Point p, Point p1, Point p2) {
            double A = p2.Y - p1.Y;
            double B = p2.X - p1.X;

            return Math.Abs(A * (p1.X - p.X) + B * (p.Y - p1.Y)) / Math.Sqrt(A * A + B * B);
        }


        /// <summary>
        /// This function calculates the lenght of the given edge
        /// </summary>
        /// <param name="edge"> Edge which length is to be calculated</param>
        private double edgeLength(Edge edge) {
            double lengthSq = Math.Pow(vertices[edge.A.X][edge.A.Y].X - vertices[edge.B.X][edge.B.Y].X, 2) + Math.Pow(vertices[edge.A.X][edge.A.Y].Y - vertices[edge.B.X][edge.B.Y].Y, 2);
            return Math.Sqrt(lengthSq);
        }


        /// <summary>
        /// This function returns a reference to a relation or null if there is no relation assigned to the edge
        /// </summary>
        /// <param name="edge"> Edge which relation is to be returned</param>
        private Relation getRelation(Edge edge) {
            for(int i=0; i<relations.Count; i++) {
                //if(relations[i].M == edge || relations[i].N == edge) {
                if(((relations[i].M.A == edge.A && relations[i].M.B == edge.B) || (relations[i].N.A == edge.A && relations[i].N.B == edge.B))
                    || ((relations[i].M.A == edge.B && relations[i].M.B == edge.A) || (relations[i].N.A == edge.B && relations[i].N.B == edge.A))) {
                    return relations[i];
                }
            }

            return null;
        }


        /// <summary>
        /// This function updates a polygon in a way to keep proper relations between its edges
        /// </summary>
        /// <param name="vertex"> Starting vertex for the updating algorithm</param>
        /// <param name="edge"> Starting edge for the updating algorithm</param>
        /// <param name="backward"> Boolean specifying whether the updating algorithm should proceed clockwise or anti-clockwise</param>
        private void updatePolygon(MyPoint vertex, Edge edge, bool backward = false) {
            MyPoint first;
            MyPoint last;

            if (vertex != null) {
                first = vertex;
                last = vertex;
            }
            else {
                if(edge.A.Y == 0 && edge.B.Y == vertices[edge.A.X].Count - 1) {
                    first = new MyPoint(edge.A);
                    last = new MyPoint(edge.B);
                }
                else if(edge.B.Y == 0 && edge.A.Y == vertices[edge.A.X].Count - 1) {
                    first = new MyPoint(edge.B);
                    last = new MyPoint(edge.A);
                }
                else if (edge.A.Y < edge.B.Y) {
                    first = new MyPoint(edge.B);
                    last = new MyPoint(edge.A);
                }
                else {
                    first = new MyPoint(edge.A);
                    last = new MyPoint(edge.B);
                }
            }

            int i = first.Y;
            if(backward) {
                i = last.Y;
            }

            bool further = false;
            do {
                int next = i + 1;
                if(backward) {
                    next = i - 1;
                }

                if (!backward && next == vertices[first.X].Count) {
                    next = 0;
                }
                else if(backward && next == -1) {
                    next = vertices[first.X].Count - 1;
                }

                Relation relation = getRelation(new Edge(new Point(first.X, i), new Point(first.X, next)));
                if (relation != null) {
                    Edge M = relation.M;
                    Edge N = relation.N;
                    Edge currEdge = new Edge(new Point(first.X, i), new Point(first.X, next));
                    Edge secondEdge = null;

                    if((M.A.Y == i && M.B.Y == next) || (M.A.Y == next && M.B.Y == i)) {
                        secondEdge = N;
                    }
                    else if((N.A.Y == i && N.B.Y == next) || (N.A.Y == next && N.B.Y == i)) {
                        secondEdge = M;
                    }
                    else {
                        MessageBox.Show("Error while choosing secondEdge of the relation");
                    }

                    if(relation.type == 6 && Math.Abs(edgeLength(M) - edgeLength(N)) > 0.001) {
                        double currEdgeLength = edgeLength(currEdge);
                        double secondEdgeLength = edgeLength(secondEdge);
                        double scale = secondEdgeLength / currEdgeLength;

                        int newX = (int)(vertices[first.X][next].X + (scale - 1) * (vertices[first.X][next].X - vertices[first.X][i].X));
                        int newY = (int)(vertices[first.X][next].Y + (scale - 1) * (vertices[first.X][next].Y - vertices[first.X][i].Y));

                        vertices[first.X][next] = new Point(newX, newY);
                    }

                    else if(relation.type == 7) {
                        int dx1 = vertices[first.X][M.A.Y].X - vertices[first.X][M.B.Y].X;
                        int dy1 = vertices[first.X][M.A.Y].Y - vertices[first.X][M.B.Y].Y;
                        int dx2 = vertices[first.X][N.A.Y].X - vertices[first.X][N.B.Y].X;
                        int dy2 = vertices[first.X][N.A.Y].Y - vertices[first.X][N.B.Y].Y;
                        double cosAngle = Math.Abs((dx1 * dx2 + dy1 * dy2) / Math.Sqrt((dx1 * dx1 + dy1 * dy1) * (dx2 * dx2 + dy2 * dy2)));
                        
                        if(cosAngle < 0.99 || true) {
                            int secondEdgeDiffX = vertices[first.X][secondEdge.A.Y].X - vertices[first.X][secondEdge.B.Y].X;
                            int secondEdgeDiffY = vertices[first.X][secondEdge.A.Y].Y - vertices[first.X][secondEdge.B.Y].Y;

                            vertices[first.X][next] = new Point(vertices[first.X][i].X + secondEdgeDiffX, vertices[first.X][i].Y + secondEdgeDiffY);
                        }
                    }
                }

                i = next;

                further = false;
                if(!backward && i != last.Y) {
                    further = true;
                }
                else if(backward && i != first.Y) {
                    further = true;
                }
            } while (further);

            //check if it's correct
            for(int k=0; k<relations.Count; k++) {
                if(relations[k].type == 6 && Math.Abs(edgeLength(relations[k].M) - edgeLength(relations[k].N)) > 4) {
                    updatePolygon(new MyPoint(relations[k].M.A.X, 0), null, true);

                    if(relations[k].type == 6 && Math.Abs(edgeLength(relations[k].M) - edgeLength(relations[k].N)) > 4) {
                        MessageBox.Show("Błąd w relacji: E" + relations[k].number + "\nPrzesuń inny wierzchołek albo krawędź");
                        mouseIsDown = false;
                    }
                }
                if( relations[k].type == 7) {
                    Edge M = relations[k].M;
                    Edge N = relations[k].N;

                    int dx1 = vertices[M.A.X][M.A.Y].X - vertices[M.A.X][M.B.Y].X;
                    int dy1 = vertices[M.A.X][M.A.Y].Y - vertices[M.A.X][M.B.Y].Y;
                    int dx2 = vertices[M.A.X][N.A.Y].X - vertices[M.A.X][N.B.Y].X;
                    int dy2 = vertices[M.A.X][N.A.Y].Y - vertices[M.A.X][N.B.Y].Y;
                    double cosAngle = Math.Abs((dx1 * dx2 + dy1 * dy2) / Math.Sqrt((dx1 * dx1 + dy1 * dy1) * (dx2 * dx2 + dy2 * dy2)));

                    if(cosAngle < 0.95) {
                        updatePolygon(new MyPoint(relations[k].M.A.X, 0), null, true);

                        dx1 = vertices[M.A.X][M.A.Y].X - vertices[M.A.X][M.B.Y].X;
                        dy1 = vertices[M.A.X][M.A.Y].Y - vertices[M.A.X][M.B.Y].Y;
                        dx2 = vertices[M.A.X][N.A.Y].X - vertices[M.A.X][N.B.Y].X;
                        dy2 = vertices[M.A.X][N.A.Y].Y - vertices[M.A.X][N.B.Y].Y;
                        cosAngle = Math.Abs((dx1 * dx2 + dy1 * dy2) / Math.Sqrt((dx1 * dx1 + dy1 * dy1) * (dx2 * dx2 + dy2 * dy2)));

                        if(cosAngle < 0.95) {
                            MessageBox.Show("Error in the relation: P" + relations[k].number + "\nMove another vertex or edge");
                            mouseIsDown = false;
                        }
                    }
                }
            }
        }
    }
}
