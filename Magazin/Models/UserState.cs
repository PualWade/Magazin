namespace Magazin.Models
{
    public enum UserState
    {
        Idle,           // Ожидание команды
        WaitingForFile  // Ожидание файла
        // В дальнейшем можно добавить другие состояния
    }
}
