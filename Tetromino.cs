using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Tetris;

public class Tetromino
{
    public static List<Tetromino> BlockList = ReadFromJSON();


    public Tetromino(int[,] shape, Color color)
    {
        Shape = shape;
        Color = color;
        Position = Vector2.Zero;
        Rotation = Rotation.Rotate0;
    }

    public Tetromino(Tetromino other)
    {
        Shape = (int[,])other.Shape.Clone();
        Color = other.Color;
        Position = other.Position;
        Rotation = other.Rotation;
    }

    public int[,] Shape { get; set; }
    public Color Color { get; set; }
    public Vector2 Position { get; set; }
    public Rotation Rotation { get; set; }

    public int Width => Shape.GetLength(1);
    public int Height => Shape.GetLength(0);

    public void Rotate(bool clockwise = true)
    {
        var rows = Shape.GetLength(0);
        var cols = Shape.GetLength(1);
        var rotated = new int[cols, rows];

        for (var i = 0; i < rows; i++)
        for (var j = 0; j < cols; j++)
            if (clockwise)
                rotated[j, rows - 1 - i] = Shape[i, j];
            else
                rotated[cols - 1 - j, i] = Shape[i, j];

        Shape = rotated;
        Rotation = (Rotation)((int)(Rotation + (clockwise ? 1 : 3)) % 4);
    }

    public void Move(int dx, int dy)
    {
        Position = new Vector2(Position.X + dx, Position.Y + dy);
    }

    public static Tetromino GetRandomTetromino()
    {
        if (BlockList.Count == 0) throw new InvalidOperationException("No tetrominos available in the block list.");
        var rnd = new Random();
        var index = rnd.Next(BlockList.Count);
        var selectedTetromino = BlockList[index];

        var newTetromino = new Tetromino(selectedTetromino.Shape, selectedTetromino.Color)
        {
            Position = new Vector2(GameBoard.GridWidth / 2 - selectedTetromino.Shape.GetLength(1) / 2, 0)
        };

        return newTetromino;
    }

    public static List<Tetromino> ReadFromJSON()
    {
        List<JSONBlock> list;
        list = JsonConvert.DeserializeObject<List<JSONBlock>>(File.ReadAllText("blocks.json"));
        var blockListe = new List<Tetromino>();

        foreach (var JBlock in list)
            blockListe.Add(new Tetromino(JBlock.Layout, new Color(JBlock.Color[0], JBlock.Color[1], JBlock.Color[2])));
        return blockListe;
    }

    public struct JSONBlock
    {
        public int[] Color;
        public int[,] Layout;
    }
}