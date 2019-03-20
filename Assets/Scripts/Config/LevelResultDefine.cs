namespace Config
{
    public enum LevelResult
    {
        Pass           // 通过
      , FailToFindNode // 未找到BubbType.Empty的Node
      , FailToMoveDown // 不能下移
    }
}