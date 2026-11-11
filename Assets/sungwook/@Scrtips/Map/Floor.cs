public class Floor
{
    public FloorType[,,] floorDataArr { get; private set; }
    private FloorDataSO floorDataSO;

    public Floor(FloorDataSO floorDataSO)
    {
        this.floorDataSO = floorDataSO;
        ConfigureFloorData();
    }

    private void ConfigureFloorData()
    {
        
    }

    public void CreateFloor()
    {
        
    }

    public void SetFloorData(FloorType floorType,int x, int y, int z)
    {
        floorDataArr[x, y, z] = floorType;
    }

    public FloorType PopAndZero(int x, int y, int z)
    {
        floorDataArr[x, y, z] = FloorType.Empty;
        return floorDataArr[x, y, z];
    }
}