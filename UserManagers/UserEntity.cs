namespace AGVSystem.UserManagers
{
    public class UserEntity : UserLoginRequest
    {
        public enum USER_ROLE
        {
            VISITOR,
            ENG,
            DEVELOPER
        }

        public USER_ROLE Role { get; set; } = USER_ROLE.VISITOR;

    }
}
