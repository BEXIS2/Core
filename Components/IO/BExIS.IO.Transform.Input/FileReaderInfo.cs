
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BExIS.IO.Transform.Input
{

    public class FileReaderInfo
    {
        /// <summary>
        /// representation of decimal 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public DecimalCharacter Decimal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public Orientation Orientation { get; set; }

        /// <summary>
        /// Number of empty columns (columnwise) or rows (rowwise) before the variables are specified.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public int Offset { get; set; }

        /// <summary>
        /// Row/Column in which the variables are.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public int Variables { get; set; }

        /// <summary>
        /// Row/Column in which the Data are.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public int Data { get; set; }

        /// <summary>
        /// Row/Column in which the Unit are.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>  
        public int Unit { get; set; }

        /// <summary>
        /// Row/Column in which the Description are.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>  
        public int Description { get; set; }

        /// <summary>
        /// Format of the Date
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public string Dateformat { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Get the DecimalCharacter as string
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="decimalCharacter"> DecimalCharacter as enum</param>  
        /// <returns>DecimalCharacter as String</returns>
        public static string GetDecimal(DecimalCharacter decimalCharacter)
        {
            switch (decimalCharacter)
            {
                case DecimalCharacter.comma:
                    return DecimalCharacter.comma.ToString();
                case DecimalCharacter.point:
                    return DecimalCharacter.point.ToString();
                default: return DecimalCharacter.point.ToString();
            }
        }

        /// <summary>
        /// Get the DecimalCharacter as char
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="decimalCharacter"> DecimalCharacter as enum</param>  
        /// <returns>DecimalCharacter as String</returns>
        public static char GetDecimalCharacter(DecimalCharacter decimalCharacter)
        {
            switch (decimalCharacter)
            {
                case DecimalCharacter.comma:
                    return ',';
                case DecimalCharacter.point:
                    return '.';
                default: return '.';
            }
        }

        /// <summary>
        /// Get the DecimalCharacter as enum
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="decimalCharacter"> DecimalCharacter as string</param>  
        /// <returns>DecimalCharacter as enum</returns>
        public static DecimalCharacter GetDecimalCharacter(string decimalCharacter)
        {
            switch (decimalCharacter)
            {
                case "comma":
                    return DecimalCharacter.comma;
                case "point":
                    return DecimalCharacter.point;
                default: return DecimalCharacter.point;
            }
        }

        /// <summary>
        /// Get the DecimalCharacter as enum
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="decimalCharacter"> DecimalCharacter as string</param>  
        /// <returns>DecimalCharacter as enum</returns>
        public static DecimalCharacter GetDecimalCharacter(char decimalCharacter)
        {
            switch (decimalCharacter)
            {
                case ',':
                    return DecimalCharacter.comma;
                case '.':
                    return DecimalCharacter.point;
                default: return DecimalCharacter.point;
            }
        }

        /// <summary>
        /// Get the Orientation as string
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="decimalCharacter"> Orientation as enum</param>  
        /// <returns>Orientation as String</returns>
        public static string GetOrientation(Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.columnwise:
                    return Orientation.columnwise.ToString();
                case Orientation.rowwise:
                    return Orientation.rowwise.ToString();
                default: return Orientation.columnwise.ToString();
            }
        }

        /// <summary>
        /// Get the Orientation as enum
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="decimalCharacter"> Orientation as string</param>  
        /// <returns>Orientation as enum</returns>
        public static Orientation GetOrientation(string orientation)
        {
            switch (orientation)
            {
                case "columnswise":
                    return Orientation.columnwise;
                case "rowwise":
                    return Orientation.rowwise;
                default: return Orientation.columnwise;
            }
        }

    }

}
