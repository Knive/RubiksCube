using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using RubiksCube;
using System.Collections.Generic;

/// <summary>
/// Save system
/// </summary>
public static class SaveSystem
{
	/// <summary>
	/// Save moves into save file
	/// </summary>
	/// <param name="moves">Moves</param>
	public static void SaveMoves(Queue<Move> moves)
	{
		BinaryFormatter formatter = new BinaryFormatter();
		string path = Application.persistentDataPath + "/rubiks_cube.dat";
		FileStream stream = new FileStream(path, FileMode.Create);

		MoveData data = new MoveData(moves);

		formatter.Serialize(stream, data);
		stream.Close();
	}

	/// <summary>
	/// Load save file
	/// </summary>
	/// <returns>Moves data</returns>
	public static MoveData LoadMoves()
	{
		string path = Application.persistentDataPath + "/rubiks_cube.dat";
		if (File.Exists(path))
		{
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(path, FileMode.Open);

			MoveData data = formatter.Deserialize(stream) as MoveData;
			stream.Close();

			return data;
		}
		else
		{
			Debug.LogError("Save file not found in " + path);
			return null;
		}
	}

	/// <summary>
	/// Checks whether or not save file exists
	/// </summary>
	/// <returns>True or False</returns>
	public static bool SaveFileExists()
	{
		string path = Application.persistentDataPath + "/rubiks_cube.dat";
		return File.Exists(path);
	}
}
