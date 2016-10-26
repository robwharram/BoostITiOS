using System;

namespace BoostITiOS
{
	public class ImageForUpload
	{
		public int vehicleId { get; set; }
		public int dealershipId { get; set; }
		public int fileNumber { get; set; }
		public string filePath { get; set; }
		public long fileSize { get; set; }
		public int damageId { get; set; }
		public int orientation { get; set; }
	}
}

