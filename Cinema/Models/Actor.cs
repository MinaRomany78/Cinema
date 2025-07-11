﻿using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class Actor
    {
        //FirstName, LastName, Bio, ProfilePicture, News
        public int Id { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        public string Bio { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
        public string News { get; set; } = string.Empty;
        public ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();

    }
}
