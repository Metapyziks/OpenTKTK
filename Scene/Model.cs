/**
 * Copyright (c) 2013 James King [metapyziks@gmail.com]
 *
 * This file is part of OpenTKTK.
 * 
 * OpenTKTK is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * OpenTKTK is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with OpenTKTK. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using OpenTK;

using OpenTKTK.Utils;

namespace OpenTKTK.Scene
{
    /// <summary>
    /// Class containing a list of vertices grouped into faces.
    /// </summary>
    public sealed class Model :
        IDisposable
    {
        #region Private Enum VertData
        /// <summary>
        /// Enumeration of the vertex attribute types stored in each face.
        /// </summary>
        private enum VertData : byte
        {
            Vertex = 0,
            TextUV = 1,
            Normal = 2
        }
        #endregion

        #region Private Class Face
        /// <summary>
        /// Class representing a single triangular face. Contains a position, normal,
        /// and UV coordinate for each of the three vertices in the face.
        /// </summary>
        private class Face
        {
            #region Private Static Fields
            private static readonly Regex _sREGroup = new Regex("[0-9]*(/[0-9]*){2}");
            #endregion

            #region Private Fields
            private int[,] _indices;
            #endregion

            /// <summary>
            /// Parses a face from a string.
            /// </summary>
            /// <param name="str">String of form "f p1/t1/n1 p2/t2/n2 p3/t3/n3"</param>
            /// <returns></returns>
            public static Face Parse(String str)
            {
                // Create an empty 3x3 array to store the vertex indices during parsing
                var indices = new int[3, 3];
               
                int i = 0;

                // Loop through each substring matching the regex for
                // a triplet of vertex indices
                var match = _sREGroup.Match(str);
                while (match.Success) {
                    var group = match.Value;
                    int prev = 0;
                    int next = -1;
                    int j = 0;

                    // Loop through each index, parse it to an integer and store it
                    // in the indices array
                    while (j < 3) {
                        prev = next + 1;
                        next = group.IndexOf('/', prev);
                        next = next == -1 ? group.Length : next;

                        // Some indices may be omitted, if they are store a default
                        // value of -1
                        if (next - prev > 0) {
                            indices[i, j] = Int32.Parse(group.Substring(prev, next - prev), _sCultureInfo) - 1;
                        } else {
                            indices[i, j] = -1;
                        }
                        ++j;
                    }
                    match = match.NextMatch();
                    ++i;
                }

                // Return a new face using the parsed indices
                return new Face(indices);
            }

            /// <summary>
            /// Gets a specific index when given a vertex number and the index type
            /// </summary>
            /// <param name="vert">Vertex number (0, 1, or 2)</param>
            /// <param name="type">Type of the index to get</param>
            /// <returns></returns>
            public int this[int vert, VertData type]
            {
                get { return _indices[vert, (int) type]; }
            }

            /// <summary>
            /// Private constructor to create a new Face instance.
            /// </summary>
            /// <param name="indices">3x3 array of indices for this face</param>
            private Face(int[,] indices)
            {
                _indices = indices;
            }
        }
        #endregion

        #region Public Class FaceGroup
        /// <summary>
        /// Class representing a group of faces. Stores the start index and face
        /// count of the group in the parent model's list of faces.
        /// </summary>
        public class FaceGroup
        {
            /// <summary>
            /// String identifier of the face group.
            /// </summary>
            public readonly String Name;

            /// <summary>
            /// Start index of this face group in the parent model's face list.
            /// </summary>
            public int StartIndex { get; internal set; }

            /// <summary>
            /// Number of faces in this face group.
            /// </summary>
            public int Length { get; internal set; }

            /// <summary>
            /// Constructor to create a new FaceGroup,
            /// </summary>
            /// <param name="name">String identifier of the face group</param>
            public FaceGroup(String name)
            {
                Name = name;
            }
        }
        #endregion

        #region Private Static Fields
        private static readonly CultureInfo _sCultureInfo = CultureInfo.GetCultureInfo("en-US");

        private static readonly Regex _sRENumb = new Regex("-?[0-9]+(\\.[0-9]+)?");
        private static readonly Regex _sREObjc = new Regex("^o\\s.*$");
        private static readonly Regex _sREVert = new Regex("^v(\\s+-?[0-9]+(\\.[0-9]+)?){3}$");
        private static readonly Regex _sRETxUV = new Regex("^vt(\\s+-?[0-9]+(\\.[0-9]+)?){2}$");
        private static readonly Regex _sRENorm = new Regex("^vn(\\s+-?[0-9]+(\\.[0-9]+)?){3}$");
        private static readonly Regex _sREFace = new Regex("^f(\\s+[0-9]*(/[0-9]*){2}){3}$");
        #endregion

        /// <summary>
        /// Helper function to parse a 2-component vector from a string.
        /// </summary>
        /// <param name="str">String representing a 2-component vector</param>
        /// <returns>A 2-component vector parsed from the input string</returns>
        private static Vector2 ParseVector2(String str)
        {
            // Match the first two numbers found in the string, parse them to floats,
            // and store them in a vector
            var match = _sRENumb.Match(str);
            var vector = new Vector2();
            vector.X = Single.Parse(match.Value, _sCultureInfo); match = match.NextMatch();
            vector.Y = Single.Parse(match.Value, _sCultureInfo);

            // Return the parsed vector
            return vector;
        }

        /// <summary>
        /// Helper function to parse a 3-component vector from a string.
        /// </summary>
        /// <param name="str">String representing a 3-component vector</param>
        /// <returns>A 3-component vector parsed from the input string</returns>
        private static Vector3 ParseVector3(String str)
        {
            // Match the first three numbers found in the string, parse them to floats,
            // and store them in a vector
            var match = _sRENumb.Match(str);
            var vector = new Vector3();
            vector.X = Single.Parse(match.Value, _sCultureInfo); match = match.NextMatch();
            vector.Y = Single.Parse(match.Value, _sCultureInfo); match = match.NextMatch();
            vector.Z = Single.Parse(match.Value, _sCultureInfo);

            // Return the parsed vector
            return vector;
        }
        
        /// <summary>
        /// Reads a model from a Wavefront OBJ (.obj) model file.
        /// </summary>
        /// <param name="path">File path to the file to parse</param>
        /// <returns>Model parsed from the specified file</returns>
        public static Model FromFile(String path)
        {
            int vertCount = 0, txuvCount = 0, normCount = 0, faceCount = 0;
            var vertGroups = new List<FaceGroup>();
            FaceGroup lastGroup = null;

            // Read all lines from the file and store them as a string array
            var lines = File.ReadAllLines(path);

            // Loop through each line in the array, counting how many vertices,
            // normals, texture UV coordinates, and faces there are. Also, find
            // the start indices and lengths of any face groups.
            foreach (var line in lines) {
                // If the line specifies the start of a face group...
                if (_sREObjc.IsMatch(line)) {
                    // If this isn't the first group in the file...
                    if (lastGroup != null) {
                        // Update the last group to record that the group
                        // ended at the last face read
                        lastGroup.Length = faceCount - lastGroup.StartIndex;
                    }

                    // Start a new face group with the name given by this line, and
                    // with the start index being that of the next face read
                    lastGroup = new FaceGroup(line.Substring(line.IndexOf(' ') + 1)) { StartIndex = faceCount };
                    
                    // Add the face group to the list of groups in this file
                    vertGroups.Add(lastGroup);
                    continue;
                }

                // If the line specifies a vertex position, increment the vertex count
                if (_sREVert.IsMatch(line)) {
                    ++vertCount; continue;
                }

                // If the line specifies a texture coordinate, increment the UV count
                if (_sRETxUV.IsMatch(line)) {
                    ++txuvCount; continue;
                }

                // If the line specifies a vertex normal, increment the normal count
                if (_sRENorm.IsMatch(line)) {
                    ++normCount; continue;
                }

                // If the line specifies a face, increment the face count
                if (_sREFace.IsMatch(line)) {
                    ++faceCount; continue;
                }
            }

            // If there is a face group that has not been completed...
            if (lastGroup != null) {
                // Update the last group to record that the group
                // ended at the last face read
                lastGroup.Length = faceCount - lastGroup.StartIndex;
            }

            // Set up the arrays for vertex positions, UV coordinates, normals, and faces
            var verts = new Vector3[vertCount]; int vi = 0;
            var txuvs = new Vector2[txuvCount]; int ti = 0;
            var norms = new Vector3[normCount]; int ni = 0;
            var faces = new Face[faceCount]; int fi = 0;

            // Create a model instance with these arrays, which will be populated after
            var model = new Model(vertGroups.ToArray(), verts, txuvs, norms, faces);

            // Loop through each line again, but this time actually parsing vertex values
            foreach (var line in lines) {
                // Chop off the identifier token from the start of the string
                var data = line.Substring(line.IndexOf(' ') + 1);

                // If the line is a vertex position, parse it and store it
                if (_sREVert.IsMatch(line)) {
                    verts[vi++] = ParseVector3(data);
                    continue;
                }

                // If the line is a texture UV coordinate, parse it and store it
                if (_sRETxUV.IsMatch(line)) {
                    txuvs[ti++] = ParseVector2(data);
                    continue;
                }

                // If the line is a vertex normal, parse it and store it
                if (_sRENorm.IsMatch(line)) {
                    norms[ni++] = ParseVector3(data);
                    continue;
                }

                // If the line is a face, parse its indices and store it
                if (_sREFace.IsMatch(line)) {
                    faces[fi++] = Face.Parse(data);
                    continue;
                }
            }

            // Update the model's VBO with the newly parsed data, then return it
            model.UpdateVertices();
            return model;
        }

        #region Private Fields
        private readonly Vector3[] _verts;
        private readonly Vector2[] _txuvs;
        private readonly Vector3[] _norms;
        private readonly Face[] _faces;

        private VertexBuffer _vb;
        #endregion
        
        /// <summary>
        /// Array of the distinct face groups in the model.
        /// </summary>
        public readonly FaceGroup[] FaceGroups;

        /// <summary>
        /// Private constructor to create a new instance of Model.
        /// </summary>
        /// <param name="faceGroups">Array of face groups within the model</param>
        /// <param name="verts">Array of vertex positions</param>
        /// <param name="txuvs">Array of texture UV coordinates</param>
        /// <param name="norms">Array of vertex normals</param>
        /// <param name="faces">Array of faces within the model</param>
        private Model(FaceGroup[] faceGroups, Vector3[] verts, Vector2[] txuvs, Vector3[] norms, Face[] faces)
        {
            FaceGroups = faceGroups;

            _verts = verts;
            _txuvs = txuvs;
            _norms = norms;
            _faces = faces;

            // Create a vertex buffer to store the vertex data, with a stride
            // of 8 (position:3 + uv:2 + normal:3)
            _vb = new VertexBuffer(8);
        }

        /// <summary>
        /// Populate the vertex buffer with vertex data taken from the model's arrays.
        /// </summary>
        public void UpdateVertices()
        {
            // Length of the data is vertex stride * vertices per face * number of faces
            float[] raw = new float[8 * 3 * _faces.Length];
            int i = 0;

            // Loop through each face and add the face data to the array
            foreach (var face in _faces) {
                // For each of the three vertices in the face
                for (int j = 0; j < 3; ++j) {
                    // Store the vertex position
                    raw[i++] = _verts[face[j, VertData.Vertex]].X;
                    raw[i++] = _verts[face[j, VertData.Vertex]].Y;
                    raw[i++] = _verts[face[j, VertData.Vertex]].Z;

                    // Store the texture UV coordinate (if one exists)
                    if (face[j, VertData.TextUV] > -1) {
                        raw[i++] = _txuvs[face[j, VertData.TextUV]].X;
                        raw[i++] = _txuvs[face[j, VertData.TextUV]].Y;
                    } else {
                        // If there is no texture UV, store (0, 0) instead
                        raw[i++] = 0f;
                        raw[i++] = 0f;
                    }

                    // Store the vertex normals
                    raw[i++] = _norms[face[j, VertData.Normal]].X;
                    raw[i++] = _norms[face[j, VertData.Normal]].Y;
                    raw[i++] = _norms[face[j, VertData.Normal]].Z;
                }
            }

            // Finally, send the data to the vertex buffer
            _vb.SetData(raw);
        }

        /// <summary>
        /// Gets an array of face groups which have names that match one or more prefixes.
        /// </summary>
        /// <param name="prefixes">One or more prefixes to match</param>
        /// <returns>Array of matching face groups</returns>
        public FaceGroup[] GetFaceGroups(params String[] prefixes)
        {
            prefixes = prefixes.Select(x => x + "_").ToArray();
            return FaceGroups.Where(x => prefixes.Any(y => x.Name.StartsWith(y))).ToArray();
        }

        /// <summary>
        /// Dispose of any unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _vb.Dispose();
        }
    }
}
