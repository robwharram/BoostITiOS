using System;
using System.IO;
using SQLite;
using BoostIT.Models;

namespace BoostITiOS
{
	internal static class SQLiteBoostDB
	{
		internal static string GetDBPath()
		{
			//string dbpath = (Environment.SpecialFolder.Personal)
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string libraryPath = Path.Combine (documentsPath, "..", "Library");
			string databasePath = Path.Combine(libraryPath, "Database");
			if (!Directory.Exists(databasePath))
				Directory.CreateDirectory(databasePath);

			return Path.Combine(databasePath, "Boost.db3");
		}

		// This method checks to see if the database exists, and if it doesn't, it creates
		// it and inserts some data
		internal static void CheckAndCreateDatabase()
		{			
			// create a connection object. if the database doesn't exist, it will create 
			// a blank database
			string dbPath = GetDBPath();
			using (Connection db = new Connection(dbPath))
			{
				// create the tables
				db.CreateTable<Vehicle>();
				//db.CreateTable<User>();
				db.CreateTable<AvailableFeature>();
				db.CreateTable<Body>();
				db.CreateTable<Feature>();
				db.CreateTable<Image>();
				db.CreateTable<Make>();
				db.CreateTable<Model>();
				db.CreateTable<Drivetrain>();
				db.CreateTable<ExteriorColour>();
				db.CreateTable<InteriorColour>();
				db.CreateTable<Doors>();
				db.CreateTable<Seats>();
				db.CreateTable<Transmission>();
				db.CreateTable<Dealership>();
				db.CreateTable<Damage>();
				db.CreateTable<DamageArea>();
				db.CreateTable<DamageAreaType>();
				db.CreateTable<DamageType>();
				db.CreateTable<BoostIT.Models.Paint>();
				db.CreateTable<Tire>();
				db.CreateTable<Upload>();
				db.CreateTable<UploadDealer>();
				db.CreateTable<UploadDealerVehicles>();

				// close the connection
				db.Close();
			}

			//if (File.Exists(dbPath) && !NSFileManager.GetSkipBackupAttribute(dbPath))
			// NSFileManager.SetSkipBackupAttribute(dbPath, true);
		}
	}
}

