using System.ComponentModel.DataAnnotations;

namespace ValleyVisionSolution.Pages.DataClasses
{
    public class FullProfile
    {
        public int UserID { get; set; }
        
        public string Password { get; set; }

        [Required(ErrorMessage="Username is required.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone {  get; set; }
        [Required(ErrorMessage = "User Type is required.")]
        public string UserType { get; set; }
        public string Street { get; set; }
        public string? Apartment { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int Zip { get; set; }
        public string Country { get; set; }

        


    }
}
