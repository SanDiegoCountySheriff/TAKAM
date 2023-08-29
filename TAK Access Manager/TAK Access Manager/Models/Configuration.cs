using Azure.Core.Pipeline;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TAK_Access_Manager.Models;

public partial class Configuration
{
    [Key]
    public int? ConfigId { get; set; }
    public string? Subject { get; set; }
    public string? GroupIds { get; set; }
    public string? ConfiguredBy { get; set; }
    public DateTime? ConfigureDate { get; set; }     
    public DateTime? ExpirationDate { get; set; }
    public string? Notes { get; set; }
    public string? Server { get; set; }
    public int? StatusCid { get; set; }
    public int? PackageId { get; set; }
    public Guid? UserId { get; set; }
    public string? FileName { get; set; }
    public string? BlobPath { get; set; }
    public int? ConfigType { get; set; }
}
