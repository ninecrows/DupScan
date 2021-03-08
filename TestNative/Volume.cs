namespace C9Native
{
    /// <summary>
    /// Load all available volume information for the specified volume.
    /// </summary>
    public class Volume
    {
        /// <summary>
        /// Retrieve the volume path that was used to populate this object.
        /// </summary>
        public string VolumePath { get; }

        /// <summary>
        /// Retrieve mount paths for this volume.
        /// </summary>
        public VolumePathNames Names { get; }

        /// <summary>
        /// Retrieve the type information for this volume.
        /// </summary>
        public VolumeType Type { get; }

        /// <summary>
        /// Retrieve space information for this volume.
        /// </summary>
        public VolumeSpace Space { get; }

        /// <summary>
        /// Retrieve label serial number and related volume information.
        /// </summary>
        public VolumeInformation Information { get; }

        /// <summary>
        /// Given a volume path, retrieve all available information related to that volume.
        /// </summary>
        /// <param name="path"></param>
        public Volume(string path)
        {
            VolumePath = path;

            Names = new VolumePathNames(path);
            Type = new VolumeType(path);
            Space = new VolumeSpace(path);
            Information = new VolumeInformation(path);
        }
    }
}
