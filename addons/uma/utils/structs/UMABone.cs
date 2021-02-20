public struct UMABone
{
    public int index0 { get; set; }
    public int index1 { get; set; }
    public int index2 { get; set; }
    public int index3 { get; set; }
    public float weight0 { get; set; }
    public float weight1 { get; set; }
    public float weight2 { get; set; }
    public float weight3 { get; set; }


    public float[] getWeights()
    {
        var weights = new float[4];
        weights[0] = weight0;
        weights[1] = weight1;
        weights[2] = weight2;
        weights[3] = weight3;

        return weights;
    }
    public int[] getIndexes()
    {
        var indexes = new int[4];
        indexes[0] = index0;
        indexes[1] = index1;
        indexes[2] = index1;
        indexes[3] = index3;

        return indexes;
    }
}