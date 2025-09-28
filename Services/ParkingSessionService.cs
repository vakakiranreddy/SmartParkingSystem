using SmartParkingSystem.DTOs.ParkingSession;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Interfaces.Services;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Services
{
    public class ParkingSessionService : IParkingSessionService
    {
        private readonly IParkingSessionRepository _parkingSessionRepository;
        private readonly IParkingSlotRepository _parkingSlotRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly IParkingRateRepository _parkingRateRepository;
        private readonly IEmailNotificationService _emailNotificationService;

        public ParkingSessionService(
            IParkingSessionRepository parkingSessionRepository,
            IParkingSlotRepository parkingSlotRepository,
            IVehicleRepository vehicleRepository,
            IUserRepository userRepository,
            IGuestRepository guestRepository,
            IParkingRateRepository parkingRateRepository,
            IEmailNotificationService emailNotificationService)
        {
            _parkingSessionRepository = parkingSessionRepository;
            _parkingSlotRepository = parkingSlotRepository;
            _vehicleRepository = vehicleRepository;
            _userRepository = userRepository;
            _guestRepository = guestRepository;
            _parkingRateRepository = parkingRateRepository;
            _emailNotificationService = emailNotificationService;
        }

        // Basic CRUD methods
        public async Task<ParkingSessionResponseDto> GetByIdAsync(int id)
        {
            try
            {
                var session = await _parkingSessionRepository.GetSessionWithDetailsAsync(id);

                if (session == null)
                    throw new ArgumentException($"Parking session with Id {id} not found.");

                return await MapToParkingSessionResponseDto(session);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving parking session with Id {id}.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSessionResponseDto>> GetAllAsync()
        {
            try
            {
                var sessions = await _parkingSessionRepository.GetAllAsync();
                var sessionDtos = new List<ParkingSessionResponseDto>();

                foreach (var session in sessions)
                {
                    var dto = await MapToParkingSessionResponseDto(session);
                    sessionDtos.Add(dto);
                }

                return sessionDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all parking sessions.", ex);
            }
        }

        public async Task<ParkingSessionResponseDto> CreateAsync(StartParkingSessionDto createDto)
        {
            return await StartWalkInSessionAsync(createDto);
        }

        public async Task<ParkingSessionResponseDto> UpdateAsync(int id, UpdateParkingFeeDto updateDto)
        {
            try
            {
                var session = await _parkingSessionRepository.GetByIdAsync(id);

                if (session == null)
                    throw new ArgumentException($"Parking session with Id {id} not found.");

                session.ParkingFee = updateDto.ParkingFee;

                var updatedSession = await _parkingSessionRepository.UpdateAsync(session);
                return await MapToParkingSessionResponseDto(updatedSession);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating parking session with Id {id}.", ex);
            }
        }

      

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                // Get the session first to check slot occupancy
                var session = await _parkingSessionRepository.GetByIdAsync(id);
                if (session == null)
                    throw new ArgumentException($"Parking session with Id {id} not found.");

                // Free up the slot if session was active or reserved
                if (session.Status == SessionStatus.Active || session.Status == SessionStatus.Reserved)
                {
                    var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
                    if (slot != null && slot.IsOccupied)
                    {
                        slot.IsOccupied = false;
                        await _parkingSlotRepository.UpdateAsync(slot);
                        Console.WriteLine($"DeleteSession: Freed up slot {slot.SlotNumber} for session {id}");
                    }
                }

                // Now delete the session
                return await _parkingSessionRepository.DeleteAsync(id);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting parking session with Id {id}.", ex);
            }
        }

        

        public async Task<ParkingSessionResponseDto> BookSlotAsync(int userId, BookSlotDto bookSlotDto)
        {
            try
            {
                // Validate user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new ArgumentException($"User with Id {userId} not found.");

                // Validate vehicle exists and belongs to user
                var vehicle = await _vehicleRepository.GetByIdAsync(bookSlotDto.VehicleId);
                if (vehicle == null)
                    throw new ArgumentException($"Vehicle with Id {bookSlotDto.VehicleId} not found.");

                if (vehicle.OwnerId != userId)
                    throw new UnauthorizedAccessException("Vehicle does not belong to the user.");

                // Validate slot is available
                var slot = await _parkingSlotRepository.GetByIdAsync(bookSlotDto.SlotId);
                if (slot == null)
                    throw new ArgumentException($"Parking slot with Id {bookSlotDto.SlotId} not found.");

                if (slot.IsOccupied || !slot.IsActive)
                    throw new InvalidOperationException("Parking slot is not available for booking.");

                // Check if vehicle already has an active session
                if (await _parkingSessionRepository.HasActiveSessionAsync(bookSlotDto.VehicleId))
                    throw new InvalidOperationException("Vehicle already has an active parking session.");

                // Create reservation session
                var session = new ParkingSession
                {
                    VehicleId = bookSlotDto.VehicleId,
                    SlotId = bookSlotDto.SlotId,
                    UserId = userId,
                    EntryTime = bookSlotDto.PlannedEntryTime,
                    ExitTime = bookSlotDto.PlannedExitTime,  // ← Add this back
                    Status = SessionStatus.Reserved,
                    ReservedTime = DateTime.UtcNow,
                    PaymentStatus = PaymentStatus.Pending,
                    ParkingFee = 0
                };

                var createdSession = await _parkingSessionRepository.AddAsync(session);

                // Send notification
                await SendSessionNotificationAsync(createdSession.Id, NotificationType.Reservation);

                return await MapToParkingSessionResponseDto(createdSession);
            }
            catch (ArgumentException) { throw; }
            catch (UnauthorizedAccessException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (Exception ex)
            {
                throw new Exception("Error booking parking slot.", ex);
            }
        }
        public async Task<ParkingSessionResponseDto> ActivateReservationAsync(int activatorUserId, ActivateReservationDto activateDto)
        {
            try
            {
                var session = await _parkingSessionRepository.GetByIdAsync(activateDto.ReservationId);
                if (session == null)
                    throw new ArgumentException($"Reservation with Id {activateDto.ReservationId} not found.");

                var activator = await _userRepository.GetByIdAsync(activatorUserId);
                if (activator == null || (activator.Role != UserRole.Admin && activator.Role != UserRole.Guard))
                    throw new UnauthorizedAccessException("Only Admin or Guard can activate reservations.");

                if (session.Status != SessionStatus.Reserved)
                    throw new InvalidOperationException("Session is not in reserved status.");

                var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
                if (slot != null)
                {
                    slot.IsOccupied = true;
                    await _parkingSlotRepository.UpdateAsync(slot);
                }

                session.Status = SessionStatus.Active;
                session.EntryTime = activateDto.ActualEntryTime ?? DateTime.UtcNow;

                var updatedSession = await _parkingSessionRepository.UpdateAsync(session);

                await SendSessionNotificationAsync(updatedSession.Id, NotificationType.Entry);

                return await MapToParkingSessionResponseDto(updatedSession);
            }
            catch (ArgumentException) { throw; }
            catch (UnauthorizedAccessException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (Exception ex) { throw new Exception("Error activating reservation.", ex); }
        }

        public async Task<IEnumerable<ReservationDto>> GetUserReservationsAsync(int userId)
        {
            try
            {
                var reservations = await _parkingSessionRepository.GetUserReservationsAsync(userId);
                var reservationDtos = new List<ReservationDto>();

                foreach (var reservation in reservations)
                {
                    var dto = await MapToReservationDto(reservation);
                    reservationDtos.Add(dto);
                }

                return reservationDtos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving reservations for user Id {userId}.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSessionResponseDto>> GetUserSessionsAsync(int userId)
        {
            try
            {
                var sessions = await _parkingSessionRepository.GetByUserIdAsync(userId);
                var sessionDtos = new List<ParkingSessionResponseDto>();

                foreach (var session in sessions)
                {
                    var dto = await MapToParkingSessionResponseDto(session);
                    sessionDtos.Add(dto);
                }

                return sessionDtos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving sessions for user Id {userId}.", ex);
            }
        }

        public async Task<ParkingSessionResponseDto> StartGuestSessionAsync(StartGuestSessionDto guestDto)
        
        
        
        
        {
            try
            {
                // Validate slot is available
                var slot = await _parkingSlotRepository.GetByIdAsync(guestDto.SlotId);
                if (slot == null)
                    throw new ArgumentException($"Parking slot with Id {guestDto.SlotId} not found.");

                if (slot.IsOccupied || !slot.IsActive)
                    throw new InvalidOperationException("Parking slot is not available.");

                // Create guest record
                var guest = new Guest
                {
                    FirstName = guestDto.FirstName,
                    LastName = guestDto.LastName,
                    PhoneNumber = guestDto.PhoneNumber,
                    Email = guestDto.Email,
                    LicensePlate = guestDto.LicensePlate,
                    VehicleType = guestDto.VehicleType,
                    Brand = guestDto.Brand,
                    Model = guestDto.Model,
                    Color = guestDto.Color
                };

                var createdGuest = await _guestRepository.AddAsync(guest);

                // Update slot occupancy
                slot.IsOccupied = true;
                await _parkingSlotRepository.UpdateAsync(slot);

                // Create guest session
                var session = new ParkingSession
                {
                    VehicleId = null,
                    SlotId = guestDto.SlotId,
                    UserId = null,
                    GuestId = createdGuest.Id,
                    EntryTime = DateTime.UtcNow,
                    Status = SessionStatus.Active,
                    PaymentStatus = PaymentStatus.Pending,
                    ParkingFee = 0
                };

                var createdSession = await _parkingSessionRepository.AddAsync(session);

                // Send entry notification to guest
                await SendSessionNotificationAsync(createdSession.Id, NotificationType.Entry);

                return await MapToParkingSessionResponseDto(createdSession);
            }
            catch (ArgumentException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (Exception ex)
            {
                throw new Exception("Error starting guest session.", ex);
            }
        }

        public async Task<bool> CancelReservationAsync(int userId, int sessionId)
        {
            try
            {
                var session = await _parkingSessionRepository.GetByIdAsync(sessionId);

                if (session == null)
                    throw new ArgumentException($"Session with Id {sessionId} not found.");

                if (session.UserId != userId)
                    throw new UnauthorizedAccessException("Session does not belong to the user.");

                if (session.Status != SessionStatus.Reserved)
                    throw new InvalidOperationException("Only reserved sessions can be cancelled.");

                session.Status = SessionStatus.Cancelled;
                await _parkingSessionRepository.UpdateAsync(session);

                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error cancelling reservation.", ex);
            }
        }

        // Guard operations
        public async Task<ParkingSessionResponseDto> StartWalkInSessionAsync(StartParkingSessionDto startSessionDto)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(startSessionDto.VehicleId);
                if (vehicle == null)
                    throw new ArgumentException($"Vehicle with Id {startSessionDto.VehicleId} not found.");

                var slot = await _parkingSlotRepository.GetByIdAsync(startSessionDto.SlotId);
                if (slot == null)
                    throw new ArgumentException($"Parking slot with Id {startSessionDto.SlotId} not found.");

                if (slot.IsOccupied || !slot.IsActive)
                    throw new InvalidOperationException("Parking slot is not available.");

                if (await _parkingSessionRepository.HasActiveSessionAsync(startSessionDto.VehicleId))
                    throw new InvalidOperationException("Vehicle already has an active parking session.");

                slot.IsOccupied = true;
                await _parkingSlotRepository.UpdateAsync(slot);

                var session = new ParkingSession
                {
                    VehicleId = startSessionDto.VehicleId,
                    SlotId = startSessionDto.SlotId,
                    UserId = vehicle.OwnerId,
                    EntryTime = DateTime.UtcNow,
                    Status = SessionStatus.Active,
                    PaymentStatus = PaymentStatus.Pending,
                    ParkingFee = 0
                };

                var createdSession = await _parkingSessionRepository.AddAsync(session);

                //_ = Task.Run(async () => {
                //    await SendSessionNotificationAsync(createdSession.Id, NotificationType.Entry);
                //});
                await SendSessionNotificationAsync(createdSession.Id, NotificationType.Entry);

                return await MapToParkingSessionResponseDto(createdSession);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error starting walk-in session.", ex);
            }
        }

        public async Task<ParkingSessionResponseDto> EndSessionAsync(EndParkingSessionDto endDto)
        {
            try
            {
                var session = await _parkingSessionRepository.GetByIdAsync(endDto.SessionId);
                if (session == null)
                    throw new ArgumentException($"Session with Id {endDto.SessionId} not found.");

                if (session.Status != SessionStatus.Active)
                    throw new InvalidOperationException("Session is not active.");

                // Update session
                session.ExitTime = endDto.ExitTime ?? DateTime.UtcNow;
                session.Status = SessionStatus.Completed;
                session.ParkingFee = await CalculateFeeForSession(session);

                // Free up the slot
                var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
                if (slot != null)
                {
                    slot.IsOccupied = false;
                    await _parkingSlotRepository.UpdateAsync(slot);
                }

                var updatedSession = await _parkingSessionRepository.UpdateAsync(session);

                // Send exit notification (works for both users and guests)
                await SendSessionNotificationAsync(updatedSession.Id, NotificationType.Exit);

                return await MapToParkingSessionResponseDto(updatedSession);
            }
            catch (ArgumentException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (Exception ex)
            {
                throw new Exception("Error ending session.", ex);
            }
        }
        // Payment operations
        public async Task<bool> ProcessPaymentAsync(int sessionId, PaymentStatus paymentStatus)
        {
            try
            {
                var session = await _parkingSessionRepository.GetByIdAsync(sessionId);

                if (session == null)
                    throw new ArgumentException($"Session with Id {sessionId} not found.");

                session.PaymentStatus = paymentStatus;
                await _parkingSessionRepository.UpdateAsync(session);

                if (paymentStatus == PaymentStatus.Paid)
                {
                    _ = Task.Run(async () => {
                        await SendSessionNotificationAsync(sessionId, NotificationType.Payment);
                    });
                }

                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing payment.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSessionResponseDto>> GetUnpaidSessionsAsync()
        {
            try
            {
                var sessions = await _parkingSessionRepository.GetUnpaidSessionsAsync();
                var sessionDtos = new List<ParkingSessionResponseDto>();

                foreach (var session in sessions)
                {
                    var dto = await MapToParkingSessionResponseDto(session);
                    sessionDtos.Add(dto);
                }

                return sessionDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving unpaid sessions.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSessionResponseDto>> GetAllActiveSessionsAsync()
        {
            try
            {
                var sessions = await _parkingSessionRepository.GetActiveSessionsAsync();
                var sessionDtos = new List<ParkingSessionResponseDto>();

                foreach (var session in sessions)
                {
                    var dto = await MapToParkingSessionResponseDto(session);
                    sessionDtos.Add(dto);
                }

                return sessionDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving active sessions.", ex);
            }
        }

        public async Task<IEnumerable<ParkingSessionResponseDto>> GetAllReservationsAsync()
        {
            try
            {
                var sessions = await _parkingSessionRepository.GetReservationsAsync();
                var sessionDtos = new List<ParkingSessionResponseDto>();

                foreach (var session in sessions)
                {
                    var dto = await MapToParkingSessionResponseDto(session);
                    sessionDtos.Add(dto);
                }

                return sessionDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving reservations.", ex);
            }
        }

        public async Task<bool> CancelSessionAsync(int sessionId)
        {
            try
            {
                var session = await _parkingSessionRepository.GetByIdAsync(sessionId);

                if (session == null)
                    throw new ArgumentException($"Session with Id {sessionId} not found.");

                if (session.Status == SessionStatus.Active || session.Status == SessionStatus.Reserved)
                {
                    var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
                    if (slot != null && slot.IsOccupied)
                    {
                        slot.IsOccupied = false;
                        await _parkingSlotRepository.UpdateAsync(slot);
                    }
                }

                session.Status = SessionStatus.Cancelled;
                await _parkingSessionRepository.UpdateAsync(session);

                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error cancelling session.", ex);
            }
        }

        // Interface implementation for fee calculation
        public async Task<decimal> CalculateParkingFeeAsync(int sessionId)
        {
            try
            {
                var session = await _parkingSessionRepository.GetByIdAsync(sessionId);
                if (session == null)
                    throw new ArgumentException($"Session with Id {sessionId} not found.");

                return await CalculateFeeForSession(session);
            }
            catch (Exception ex)
            {
                throw new Exception("Error calculating parking fee.", ex);
            }
        }

        // Private helper method for fee calculation
        private async Task<decimal> CalculateFeeForSession(ParkingSession session)
        {
            try
            {
                VehicleType vehicleType;

                if (session.VehicleId.HasValue)
                {
                    var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId.Value);
                    if (vehicle == null) return 0;
                    vehicleType = vehicle.VehicleType;
                }
                else if (session.GuestId.HasValue)
                {
                    var guest = await _guestRepository.GetByIdAsync(session.GuestId.Value);
                    if (guest == null) return 0;
                    vehicleType = guest.VehicleType;
                }
                else
                {
                    return 0;
                }

                var rate = await _parkingRateRepository.GetByVehicleTypeAsync(vehicleType);
                if (rate == null) return 0;

                var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
                if (slot == null) return 0;

                decimal totalPriceModifier = 1.0m;
                if (slot.SlotFeatures != null && slot.SlotFeatures.Any())
                {
                    foreach (var slotFeature in slot.SlotFeatures)
                    {
                        if (slotFeature.Feature != null)
                        {
                            totalPriceModifier += slotFeature.Feature.PriceModifier;
                        }
                    }
                }

                var exitTime = session.ExitTime ?? DateTime.UtcNow;
                var duration = exitTime - session.EntryTime;
                var hours = Math.Ceiling(duration.TotalHours);

                var baseFee = (decimal)hours * rate.HourlyRate;
                return baseFee * totalPriceModifier;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        // Dashboard/Reports
        public async Task<int> GetTotalActiveSlotsAsync()
        {
            try
            {
                var slots = await _parkingSlotRepository.GetAllAsync();
                return slots.Count(s => s.IsActive);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving total active slots.", ex);
            }
        }

        public async Task<int> GetTotalOccupiedSlotsAsync()
        {
            try
            {
                return await _parkingSessionRepository.GetOccupiedSlotsCountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving total occupied slots.", ex);
            }
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                return await _parkingSessionRepository.GetTotalRevenueAsync(fromDate, toDate);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving total revenue.", ex);
            }
        }

        public async Task<int> GetTotalActiveSessionsCountAsync()
        {
            try
            {
                return await _parkingSessionRepository.GetActiveSessionsCountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving active sessions count.", ex);
            }
        }

        public async Task<int> GetTotalReservationsCountAsync()
        {
            try
            {
                var reservations = await _parkingSessionRepository.GetReservationsAsync();
                return reservations.Count();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving reservations count.", ex);
            }
        }

        // Helper method to send session notifications
        private async Task SendSessionNotificationAsync(int sessionId, NotificationType notificationType)
        {
            try
            {
                var session = await _parkingSessionRepository.GetSessionWithDetailsAsync(sessionId);
                if (session == null) return;

                // Handle registered user notifications
                if (session.UserId.HasValue && session.User != null)
                {
                    var (subject, message) = GenerateEmailContent(session, notificationType);

                    var emailDto = new DTOs.EmailNotification.SendEmailNotificationDto
                    {
                        UserId = session.UserId.Value,
                        ParkingSessionId = sessionId,
                        EmailAddress = session.User.Email,
                        Subject = subject,
                        Message = message,
                        NotificationType = notificationType
                    };

                    await _emailNotificationService.SendNotificationAsync(emailDto);
                }
                // Handle guest notifications
                else if (session.GuestId.HasValue && session.Guest != null && !string.IsNullOrEmpty(session.Guest.Email))
                {
                    await SendGuestNotificationAsync(session, notificationType);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending notification: {ex.Message}");
            }
        }

        // Helper method for guest notifications
        private async Task SendGuestNotificationAsync(ParkingSession session, NotificationType notificationType)
        {
            try
            {
                if (session.Guest == null || string.IsNullOrEmpty(session.Guest.Email))
                {
                    Console.WriteLine("SendGuestNotification: No guest or email found, skipping notification");
                    return;
                }

                var guestName = $"{session.Guest.FirstName} {session.Guest.LastName}";
                var (subject, message) = GenerateGuestEmailContent(session, notificationType);

                Console.WriteLine($"SendGuestNotification: Sending {notificationType} notification to {session.Guest.Email}");

                // Send email directly without storing in database
                var emailSent = await _emailNotificationService.SendGuestEmailAsync(
                    session.Guest.Email,
                    guestName,
                    subject,
                    message
                );

                if (emailSent)
                {
                    Console.WriteLine($"SendGuestNotification: Successfully sent {notificationType} email to guest {guestName}");
                }
                else
                {
                    Console.WriteLine($"SendGuestNotification: Failed to send {notificationType} email to guest {guestName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendGuestNotification ERROR: {ex.Message}");
                // Don't throw - guest notification failure shouldn't break the main flow
            }
        }

        // Generate email content for guests
        private (string subject, string message) GenerateGuestEmailContent(ParkingSession session, NotificationType notificationType)
        {
            var guestName = $"{session.Guest.FirstName} {session.Guest.LastName}";
            var slotNumber = session.ParkingSlot?.SlotNumber ?? "N/A";
            var licensePlate = session.Guest.LicensePlate;

            return notificationType switch
            {
                NotificationType.Entry => (
                    "Parking Session Started - KR Parkings",
                    $"Dear {guestName},\n\nYour parking session has been activated.\n\nDetails:\n- Slot: {slotNumber}\n- Vehicle: {licensePlate}\n- Entry Time: {session.EntryTime:yyyy-MM-dd HH:mm}\n\nThank you for choosing KR Parkings!"
                ),
                NotificationType.Exit => (
                    "Parking Session Completed - KR Parkings",
                    $"Dear {guestName},\n\nYour parking session has ended.\n\nDetails:\n- Slot: {slotNumber}\n- Vehicle: {licensePlate}\n- Exit Time: {session.ExitTime:yyyy-MM-dd HH:mm}\n- Total Fee: ${session.ParkingFee:F2}\n\nThank you for using KR Parkings!"
                ),
                _ => ("KR Parkings Notification", $"Dear {guestName},\n\nThis is a notification regarding your parking session for vehicle {licensePlate}.")
            };
        }

        private (string subject, string message) GenerateEmailContent(ParkingSession session, NotificationType notificationType)
        {
            var userName = $"{session.User?.FirstName} {session.User?.LastName}";
            var slotNumber = session.ParkingSlot?.SlotNumber ?? "Unknown";
            var licensePlate = session.Vehicle?.LicensePlate ?? "Unknown";

            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var entryTimeIst = TimeZoneInfo.ConvertTimeFromUtc(session.EntryTime, istTimeZone);
            var reservedTimeIst = session.ReservedTime.HasValue
                ? TimeZoneInfo.ConvertTimeFromUtc(session.ReservedTime.Value, istTimeZone)
                : (DateTime?)null;
            var exitTimeIst = session.ExitTime.HasValue
                ? TimeZoneInfo.ConvertTimeFromUtc(session.ExitTime.Value, istTimeZone)
                : (DateTime?)null;

            return notificationType switch
            {
                NotificationType.Reservation => (
                    "Reservation Confirmed - Smart Parking System",
                    $"Dear {userName}, your parking reservation is confirmed.\n\n" +
                    $"Vehicle: {licensePlate}\n" +
                    $"Slot: {slotNumber}\n" +
                    $"Planned Entry (IST): {entryTimeIst:yyyy-MM-dd HH:mm}\n" +
                    (exitTimeIst.HasValue ? $"Planned Exit (IST): {exitTimeIst:yyyy-MM-dd HH:mm}\n" : "") +
                    $"Session ID: {session.Id}\n\n" +
                    "Thank you for booking with Smart Parking System!"
                ),
                NotificationType.Entry => (
                    "Parking Session Started - Smart Parking System",
                    $"Dear {userName}, your parking session has started.\n\n" +
                    $"Vehicle: {licensePlate}\n" +
                    $"Slot: {slotNumber}\n" +
                    $"Entry Time (IST): {entryTimeIst:yyyy-MM-dd HH:mm}"
                ),
                NotificationType.Exit => (
                    "Parking Session Completed - Smart Parking System",
                    $"Dear {userName}, your parking session has ended.\n\n" +
                    $"Vehicle: {licensePlate}\n" +
                    $"Slot: {slotNumber}\n" +
                    $"Entry Time (IST): {entryTimeIst:yyyy-MM-dd HH:mm}\n" +
                    $"Exit Time (IST): {exitTimeIst:yyyy-MM-dd HH:mm}\n" +
                    $"Duration: {(exitTimeIst - entryTimeIst)?.ToString(@"hh\:mm")} hours\n" +
                    $"Fee: ₹{session.ParkingFee:F2}"
                ),
                NotificationType.Payment => (
                    "Payment Confirmation - Smart Parking System",
                    $"Dear {userName}, payment confirmed.\n\n" +
                    $"Vehicle: {licensePlate}\n" +
                    $"Amount Paid: ₹{session.ParkingFee:F2}\n" +
                    $"Session ID: {session.Id}"
                ),
                _ => ("Smart Parking System Notification",
                      $"Dear {userName}, notification about your parking session.")
            };
        }

        // Private mapping methods
        private async Task<ParkingSessionResponseDto> MapToParkingSessionResponseDto(ParkingSession session)
        {
            var dto = new ParkingSessionResponseDto
            {
                Id = session.Id,
                VehicleId = session.VehicleId ?? 0,
                VehicleLicensePlate = "",
                SlotId = session.SlotId,
                SlotNumber = "",
                UserId = session.UserId ?? 0,
                UserName = "",
                EntryTime = session.EntryTime,
                ExitTime = session.ExitTime,
                Status = session.Status,
                ParkingFee = session.ParkingFee,
                PaymentStatus = session.PaymentStatus
            };

            if (session.VehicleId.HasValue)
            {
                try
                {
                    var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId.Value);
                    if (vehicle != null)
                    {
                        dto.VehicleLicensePlate = vehicle.LicensePlate;
                    }
                }
                catch { }
            }
            else if (session.GuestId.HasValue)
            {
                try
                {
                    var guest = await _guestRepository.GetByIdAsync(session.GuestId.Value);
                    if (guest != null)
                    {
                        dto.VehicleLicensePlate = guest.LicensePlate;
                    }
                }
                catch { }
            }

            try
            {
                var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
                if (slot != null)
                {
                    dto.SlotNumber = slot.SlotNumber;
                }
            }
            catch { }

            if (session.UserId.HasValue)
            {
                try
                {
                    var user = await _userRepository.GetByIdAsync(session.UserId.Value);
                    if (user != null)
                    {
                        dto.UserName = $"{user.FirstName} {user.LastName}";
                    }
                }
                catch { }
            }
            else if (session.GuestId.HasValue)
            {
                try
                {
                    var guest = await _guestRepository.GetByIdAsync(session.GuestId.Value);
                    if (guest != null)
                    {
                        dto.UserName = $"{guest.FirstName} {guest.LastName} (Guest)";
                    }
                }
                catch { }
            }

            return dto;
        }

        private async Task<ReservationDto> MapToReservationDto(ParkingSession session)
        {
            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var entryTimeIst = TimeZoneInfo.ConvertTimeFromUtc(session.EntryTime, istTimeZone);
            var exitTimeIst = session.ExitTime.HasValue
                ? TimeZoneInfo.ConvertTimeFromUtc(session.ExitTime.Value, istTimeZone)
                : (DateTime?)null;

            var dto = new ReservationDto
            {
                Id = session.Id,
                VehicleLicensePlate = "",
                SlotNumber = "",
                PlannedEntryTime = entryTimeIst,
                PlannedExitTime = exitTimeIst,
                Status = session.Status,
                EstimatedFee = session.ParkingFee
            };

            if (session.VehicleId.HasValue)
            {
                try
                {
                    var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId.Value);
                    if (vehicle != null)
                    {
                        dto.VehicleLicensePlate = vehicle.LicensePlate;
                    }
                }
                catch { }
            }
            else if (session.GuestId.HasValue)
            {
                try
                {
                    var guest = await _guestRepository.GetByIdAsync(session.GuestId.Value);
                    if (guest != null)
                    {
                        dto.VehicleLicensePlate = guest.LicensePlate;
                    }
                }
                catch { }
            }

            try
            {
                var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
                if (slot != null)
                {
                    dto.SlotNumber = slot.SlotNumber;
                }
            }
            catch { }

            return dto;
        }
    }
}

//using SmartParkingSystem.DTOs.ParkingSession;
//using SmartParkingSystem.Interfaces.Repositories;
//using SmartParkingSystem.Interfaces.Services;
//using SmartParkingSystem.Models;

//namespace SmartParkingSystem.Services
//{
//    public class ParkingSessionService : IParkingSessionService
//    {
//        //private readonly IParkingSessionRepository _parkingSessionRepository;
//        //private readonly IParkingSlotRepository _parkingSlotRepository;
//        //private readonly IVehicleRepository _vehicleRepository;
//        //private readonly IUserRepository _userRepository;
//        //private readonly IParkingRateRepository _parkingRateRepository;
//        //private readonly IEmailNotificationService _emailNotificationService;

//        //public ParkingSessionService(
//        //    IParkingSessionRepository parkingSessionRepository,
//        //    IParkingSlotRepository parkingSlotRepository,
//        //    IVehicleRepository vehicleRepository,
//        //    IUserRepository userRepository,
//        //    IParkingRateRepository parkingRateRepository,
//        //    IEmailNotificationService emailNotificationService)
//        //{
//        //    _parkingSessionRepository = parkingSessionRepository;
//        //    _parkingSlotRepository = parkingSlotRepository;
//        //    _vehicleRepository = vehicleRepository;
//        //    _userRepository = userRepository;
//        //    _parkingRateRepository = parkingRateRepository;
//        //    _emailNotificationService = emailNotificationService;
//        //}

//        private readonly IParkingSessionRepository _parkingSessionRepository;
//        private readonly IParkingSlotRepository _parkingSlotRepository;
//        private readonly IVehicleRepository _vehicleRepository;
//        private readonly IUserRepository _userRepository;
//        private readonly IGuestRepository _guestRepository; // Add this
//        private readonly IParkingRateRepository _parkingRateRepository;
//        private readonly IEmailNotificationService _emailNotificationService;

//        public ParkingSessionService(
//            IParkingSessionRepository parkingSessionRepository,
//            IParkingSlotRepository parkingSlotRepository,
//            IVehicleRepository vehicleRepository,
//            IUserRepository userRepository,
//            IGuestRepository guestRepository, // Add this
//            IParkingRateRepository parkingRateRepository,
//            IEmailNotificationService emailNotificationService)
//        {
//            _parkingSessionRepository = parkingSessionRepository;
//            _parkingSlotRepository = parkingSlotRepository;
//            _vehicleRepository = vehicleRepository;
//            _userRepository = userRepository;
//            _guestRepository = guestRepository; // Add this
//            _parkingRateRepository = parkingRateRepository;
//            _emailNotificationService = emailNotificationService;
//        }

//        // Basic CRUD methods (unchanged)
//        public async Task<ParkingSessionResponseDto> GetByIdAsync(int id)
//        {
//            try
//            {
//                var session = await _parkingSessionRepository.GetSessionWithDetailsAsync(id);

//                if (session == null)
//                    throw new ArgumentException($"Parking session with Id {id} not found.");

//                return await MapToParkingSessionResponseDto(session);
//            }
//            catch (ArgumentException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception($"Error retrieving parking session with Id {id}.", ex);
//            }
//        }

//        public async Task<IEnumerable<ParkingSessionResponseDto>> GetAllAsync()
//        {
//            try
//            {
//                var sessions = await _parkingSessionRepository.GetAllAsync();
//                var sessionDtos = new List<ParkingSessionResponseDto>();

//                foreach (var session in sessions)
//                {
//                    var dto = await MapToParkingSessionResponseDto(session);
//                    sessionDtos.Add(dto);
//                }

//                return sessionDtos;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error retrieving all parking sessions.", ex);
//            }
//        }

//        public async Task<ParkingSessionResponseDto> CreateAsync(StartParkingSessionDto createDto)
//        {
//            return await StartWalkInSessionAsync(createDto);
//        }

//        public async Task<ParkingSessionResponseDto> UpdateAsync(int id, UpdateParkingFeeDto updateDto)
//        {
//            try
//            {
//                var session = await _parkingSessionRepository.GetByIdAsync(id);

//                if (session == null)
//                    throw new ArgumentException($"Parking session with Id {id} not found.");

//                session.ParkingFee = updateDto.ParkingFee;

//                var updatedSession = await _parkingSessionRepository.UpdateAsync(session);
//                return await MapToParkingSessionResponseDto(updatedSession);
//            }
//            catch (ArgumentException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception($"Error updating parking session with Id {id}.", ex);
//            }
//        }

//        public async Task<bool> DeleteAsync(int id)
//        {
//            try
//            {
//                var exists = await _parkingSessionRepository.ExistsAsync(id);

//                if (!exists)
//                    throw new ArgumentException($"Parking session with Id {id} not found.");

//                return await _parkingSessionRepository.DeleteAsync(id);
//            }
//            catch (ArgumentException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception($"Error deleting parking session with Id {id}.", ex);
//            }
//        }

//        // User operations
//        //public async Task<ParkingSessionResponseDto> BookSlotAsync(int userId, BookSlotDto bookSlotDto)
//        //{
//        //    try
//        //    {
//        //        // Validate user exists
//        //        var user = await _userRepository.GetByIdAsync(userId);
//        //        if (user == null)
//        //            throw new ArgumentException($"User with Id {userId} not found.");

//        //        // Validate vehicle exists and belongs to user
//        //        var vehicle = await _vehicleRepository.GetByIdAsync(bookSlotDto.VehicleId);
//        //        if (vehicle == null)
//        //            throw new ArgumentException($"Vehicle with Id {bookSlotDto.VehicleId} not found.");

//        //        if (vehicle.OwnerId != userId)
//        //            throw new UnauthorizedAccessException("Vehicle does not belong to the user.");

//        //        // Validate slot is available
//        //        var slot = await _parkingSlotRepository.GetByIdAsync(bookSlotDto.SlotId);
//        //        if (slot == null)
//        //            throw new ArgumentException($"Parking slot with Id {bookSlotDto.SlotId} not found.");

//        //        if (slot.IsOccupied || !slot.IsActive)
//        //            throw new InvalidOperationException("Parking slot is not available for booking.");

//        //        // Check if vehicle already has an active session
//        //        if (await _parkingSessionRepository.HasActiveSessionAsync(bookSlotDto.VehicleId))
//        //            throw new InvalidOperationException("Vehicle already has an active parking session.");

//        //        // Create reservation
//        //        var session = new ParkingSession
//        //        {
//        //            VehicleId = bookSlotDto.VehicleId,
//        //            SlotId = bookSlotDto.SlotId,
//        //            UserId = userId,
//        //            ReservedTime = DateTime.UtcNow,
//        //            EntryTime = bookSlotDto.PlannedEntryTime,
//        //            Status = SessionStatus.Reserved,
//        //            PaymentStatus = PaymentStatus.Pending,
//        //            ParkingFee = 0 // Will be calculated later
//        //        };

//        //        var createdSession = await _parkingSessionRepository.AddAsync(session);
//        //        return await MapToParkingSessionResponseDto(createdSession);
//        //    }
//        //    catch (ArgumentException)
//        //    {
//        //        throw;
//        //    }
//        //    catch (UnauthorizedAccessException)
//        //    {
//        //        throw;
//        //    }
//        //    catch (InvalidOperationException)
//        //    {
//        //        throw;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        throw new Exception("Error booking parking slot.", ex);
//        //    }
//        //}

//        public async Task<ParkingSessionResponseDto> BookSlotAsync(int userId, BookSlotDto bookSlotDto)
//        {
//            try
//            {
//                // Validate user exists
//                var user = await _userRepository.GetByIdAsync(userId);
//                if (user == null)
//                    throw new ArgumentException($"User with Id {userId} not found.");

//                // Validate vehicle exists and belongs to user
//                var vehicle = await _vehicleRepository.GetByIdAsync(bookSlotDto.VehicleId);
//                if (vehicle == null)
//                    throw new ArgumentException($"Vehicle with Id {bookSlotDto.VehicleId} not found.");

//                if (vehicle.OwnerId != userId)
//                    throw new UnauthorizedAccessException("Vehicle does not belong to the user.");

//                // Validate slot is available
//                var slot = await _parkingSlotRepository.GetByIdAsync(bookSlotDto.SlotId);
//                if (slot == null)
//                    throw new ArgumentException($"Parking slot with Id {bookSlotDto.SlotId} not found.");

//                if (slot.IsOccupied || !slot.IsActive)
//                    throw new InvalidOperationException("Parking slot is not available for booking.");

//                // Check if vehicle already has an active session
//                if (await _parkingSessionRepository.HasActiveSessionAsync(bookSlotDto.VehicleId))
//                    throw new InvalidOperationException("Vehicle already has an active parking session.");

//                // Create reservation session
//                var session = new ParkingSession
//                {
//                    VehicleId = bookSlotDto.VehicleId,
//                    SlotId = bookSlotDto.SlotId,
//                    UserId = userId,
//                    EntryTime = bookSlotDto.PlannedEntryTime,   // Planned entry
//                    ExitTime = bookSlotDto.PlannedExitTime,     // Optional planned exit
//                    Status = SessionStatus.Reserved,
//                    ReservedTime = DateTime.UtcNow,
//                    PaymentStatus = PaymentStatus.Pending,
//                    ParkingFee = 0
//                };

//                var createdSession = await _parkingSessionRepository.AddAsync(session);

//                // ✅ Send reservation email immediately and await
//                await SendSessionNotificationAsync(createdSession.Id, NotificationType.Reservation);

//                return await MapToParkingSessionResponseDto(createdSession);
//            }
//            catch (ArgumentException) { throw; }
//            catch (UnauthorizedAccessException) { throw; }

//            catch (Exception ex)
//            {
//                throw new Exception("Error booking parking slot.", ex);
//            }
//        }




//        public async Task<ParkingSessionResponseDto> ActivateReservationAsync(int activatorUserId, ActivateReservationDto activateDto)
//        {
//            try
//            {
//                // Fetch the reservation session
//                var session = await _parkingSessionRepository.GetByIdAsync(activateDto.ReservationId);
//                if (session == null)
//                    throw new ArgumentException($"Reservation with Id {activateDto.ReservationId} not found.");

//                // Validate activator role (Admin or Guard)
//                var activator = await _userRepository.GetByIdAsync(activatorUserId);
//                if (activator == null || (activator.Role != UserRole.Admin && activator.Role != UserRole.Guard))
//                    throw new UnauthorizedAccessException("Only Admin or Guard can activate reservations.");

//                // Ensure session is in Reserved status
//                if (session.Status != SessionStatus.Reserved)
//                    throw new InvalidOperationException("Session is not in reserved status.");

//                // Update slot occupancy
//                var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
//                if (slot != null)
//                {
//                    slot.IsOccupied = true;
//                    await _parkingSlotRepository.UpdateAsync(slot);
//                }

//                // Activate session
//                session.Status = SessionStatus.Active;
//                session.EntryTime = activateDto.ActualEntryTime ?? DateTime.UtcNow;

//                var updatedSession = await _parkingSessionRepository.UpdateAsync(session);

//                // Send entry notification to vehicle owner
//                await SendSessionNotificationAsync(updatedSession.Id, NotificationType.Entry);

//                return await MapToParkingSessionResponseDto(updatedSession);
//            }
//            catch (ArgumentException) { throw; }
//            catch (UnauthorizedAccessException) { throw; }
//            catch (InvalidOperationException) { throw; }
//            catch (Exception ex) { throw new Exception("Error activating reservation.", ex); }
//        }



//        public async Task<IEnumerable<ReservationDto>> GetUserReservationsAsync(int userId)
//        {
//            try
//            {
//                var reservations = await _parkingSessionRepository.GetUserReservationsAsync(userId);
//                var reservationDtos = new List<ReservationDto>();

//                foreach (var reservation in reservations)
//                {
//                    var dto = await MapToReservationDto(reservation);
//                    reservationDtos.Add(dto);
//                }

//                return reservationDtos;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception($"Error retrieving reservations for user Id {userId}.", ex);
//            }
//        }

//        public async Task<IEnumerable<ParkingSessionResponseDto>> GetUserSessionsAsync(int userId)
//        {
//            try
//            {
//                var sessions = await _parkingSessionRepository.GetByUserIdAsync(userId);
//                var sessionDtos = new List<ParkingSessionResponseDto>();

//                foreach (var session in sessions)
//                {
//                    var dto = await MapToParkingSessionResponseDto(session);
//                    sessionDtos.Add(dto);
//                }

//                return sessionDtos;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception($"Error retrieving sessions for user Id {userId}.", ex);
//            }
//        }
//        // Add this method to ParkingSessionService class
//        public async Task<ParkingSessionResponseDto> StartGuestSessionAsync(StartGuestSessionDto guestDto)
//        {
//            try
//            {
//                // Validate slot is available
//                var slot = await _parkingSlotRepository.GetByIdAsync(guestDto.SlotId);
//                if (slot == null)
//                    throw new ArgumentException($"Parking slot with Id {guestDto.SlotId} not found.");

//                if (slot.IsOccupied || !slot.IsActive)
//                    throw new InvalidOperationException("Parking slot is not available.");

//                // Create guest record
//                var guest = new Guest
//                {
//                    FirstName = guestDto.FirstName,
//                    LastName = guestDto.LastName,
//                    PhoneNumber = guestDto.PhoneNumber,
//                    Email = guestDto.Email,
//                    LicensePlate = guestDto.LicensePlate,
//                    VehicleType = guestDto.VehicleType,
//                    Brand = guestDto.Brand,
//                    Model = guestDto.Model,
//                    Color = guestDto.Color
//                };

//                var createdGuest = await _guestRepository.AddAsync(guest);

//                // Update slot occupancy
//                slot.IsOccupied = true;
//                await _parkingSlotRepository.UpdateAsync(slot);

//                // Create guest session
//                var session = new ParkingSession
//                {
//                    VehicleId = null,
//                    SlotId = guestDto.SlotId,
//                    UserId = null,
//                    GuestId = createdGuest.Id,
//                    EntryTime = DateTime.UtcNow,
//                    Status = SessionStatus.Active,
//                    PaymentStatus = PaymentStatus.Pending,
//                    ParkingFee = 0
//                };

//                var createdSession = await _parkingSessionRepository.AddAsync(session);
//                return await MapToParkingSessionResponseDto(createdSession);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error starting guest session.", ex);
//            }
//        }
//        public async Task<bool> CancelReservationAsync(int userId, int sessionId)
//        {
//            try
//            {
//                var session = await _parkingSessionRepository.GetByIdAsync(sessionId);

//                if (session == null)
//                    throw new ArgumentException($"Session with Id {sessionId} not found.");

//                if (session.UserId != userId)
//                    throw new UnauthorizedAccessException("Session does not belong to the user.");

//                if (session.Status != SessionStatus.Reserved)
//                    throw new InvalidOperationException("Only reserved sessions can be cancelled.");

//                session.Status = SessionStatus.Cancelled;
//                await _parkingSessionRepository.UpdateAsync(session);

//                return true;
//            }
//            catch (ArgumentException)
//            {
//                throw;
//            }
//            catch (UnauthorizedAccessException)
//            {
//                throw;
//            }
//            catch (InvalidOperationException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error cancelling reservation.", ex);
//            }
//        }

//        // Guard operations - WITH EMAIL NOTIFICATIONS
//        public async Task<ParkingSessionResponseDto> StartWalkInSessionAsync(StartParkingSessionDto startSessionDto)
//        {
//            try
//            {
//                // Validate vehicle exists
//                var vehicle = await _vehicleRepository.GetByIdAsync(startSessionDto.VehicleId);
//                if (vehicle == null)
//                    throw new ArgumentException($"Vehicle with Id {startSessionDto.VehicleId} not found.");

//                // Validate slot is available
//                var slot = await _parkingSlotRepository.GetByIdAsync(startSessionDto.SlotId);
//                if (slot == null)
//                    throw new ArgumentException($"Parking slot with Id {startSessionDto.SlotId} not found.");

//                if (slot.IsOccupied || !slot.IsActive)
//                    throw new InvalidOperationException("Parking slot is not available.");

//                // Check if vehicle already has an active session
//                if (await _parkingSessionRepository.HasActiveSessionAsync(startSessionDto.VehicleId))
//                    throw new InvalidOperationException("Vehicle already has an active parking session.");

//                // Update slot occupancy
//                slot.IsOccupied = true;
//                await _parkingSlotRepository.UpdateAsync(slot);

//                // Create walk-in session
//                var session = new ParkingSession
//                {
//                    VehicleId = startSessionDto.VehicleId,
//                    SlotId = startSessionDto.SlotId,
//                    UserId = vehicle.OwnerId,
//                    EntryTime = DateTime.UtcNow,
//                    Status = SessionStatus.Active,
//                    PaymentStatus = PaymentStatus.Pending,
//                    ParkingFee = 0
//                };

//                var createdSession = await _parkingSessionRepository.AddAsync(session);

//                // Send entry notification
//                _ = Task.Run(async () => {
//                    await SendSessionNotificationAsync(createdSession.Id, NotificationType.Entry);
//                });

//                return await MapToParkingSessionResponseDto(createdSession);
//            }
//            catch (ArgumentException)
//            {
//                throw;
//            }
//            catch (InvalidOperationException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error starting walk-in session.", ex);
//            }
//        }

//        //public async Task<ParkingSessionResponseDto> EndSessionAsync(EndParkingSessionDto endSessionDto)
//        //{
//        //    try
//        //    {
//        //        var session = await _parkingSessionRepository.GetByIdAsync(endSessionDto.SessionId);

//        //        if (session == null)
//        //            throw new ArgumentException($"Session with Id {endSessionDto.SessionId} not found.");

//        //        if (session.Status != SessionStatus.Active)
//        //            throw new InvalidOperationException("Only active sessions can be ended.");

//        //        // Update slot occupancy
//        //        var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
//        //        if (slot != null)
//        //        {
//        //            slot.IsOccupied = false;
//        //            await _parkingSlotRepository.UpdateAsync(slot);
//        //        }

//        //        // Calculate parking fee
//        //        var exitTime = endSessionDto.ExitTime ?? DateTime.UtcNow;
//        //        var parkingFee = await CalculateParkingFeeAsync(session.Id);

//        //        // End session
//        //        session.ExitTime = exitTime;
//        //        session.Status = SessionStatus.Completed;
//        //        session.ParkingFee = parkingFee;

//        //        var updatedSession = await _parkingSessionRepository.UpdateAsync(session);

//        //        // Send exit notification
//        //        _ = Task.Run(async () => {
//        //            await SendSessionNotificationAsync(updatedSession.Id, NotificationType.Exit);
//        //        });

//        //        return await MapToParkingSessionResponseDto(updatedSession);
//        //    }
//        //    catch (ArgumentException)
//        //    {
//        //        throw;
//        //    }
//        //    catch (InvalidOperationException)
//        //    {
//        //        throw;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        throw new Exception("Error ending session.", ex);
//        //    }
//        //}
//        public async Task<ParkingSessionResponseDto> EndSessionAsync(EndParkingSessionDto endSessionDto)
//        {
//            try
//            {
//                var session = await _parkingSessionRepository.GetByIdAsync(endSessionDto.SessionId);

//                if (session == null)
//                    throw new ArgumentException($"Session with Id {endSessionDto.SessionId} not found.");

//                if (session.Status != SessionStatus.Active)
//                    throw new InvalidOperationException("Only active sessions can be ended.");

//                // Update slot occupancy
//                var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
//                if (slot != null)
//                {
//                    slot.IsOccupied = false;
//                    await _parkingSlotRepository.UpdateAsync(slot);
//                }

//                // Set exit time FIRST
//                var exitTime = endSessionDto.ExitTime ?? DateTime.UtcNow;
//                session.ExitTime = exitTime;

//                // Calculate parking fee directly here
//                var parkingFee = await CalculateFeeForSession(session);

//                // Update session with fee and completion status
//                session.Status = SessionStatus.Completed;
//                session.ParkingFee = parkingFee;

//                var updatedSession = await _parkingSessionRepository.UpdateAsync(session);

//                // Send exit notification with calculated fee
//                await SendSessionNotificationAsync(updatedSession.Id, NotificationType.Exit);

//                return await MapToParkingSessionResponseDto(updatedSession);
//            }
//            catch (ArgumentException)
//            {
//                throw;
//            }
//            catch (InvalidOperationException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error ending session.", ex);
//            }
//        }

//        // Helper method to calculate fee for a session object
//        private async Task<decimal> CalculateFeeForSession(ParkingSession session)
//        {
//            try
//            {
//                var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId);
//                if (vehicle == null) return 0;

//                var rate = await _parkingRateRepository.GetByVehicleTypeAsync(vehicle.VehicleType);
//                if (rate == null) return 0;

//                var exitTime = session.ExitTime ?? DateTime.UtcNow;
//                var duration = exitTime - session.EntryTime;
//                var hours = Math.Ceiling(duration.TotalHours);

//                return (decimal)hours * rate.HourlyRate;
//            }
//            catch (Exception)
//            {
//                return 0;
//            }
//        }
//        // Payment operations - WITH EMAIL NOTIFICATIONS
//        public async Task<bool> ProcessPaymentAsync(int sessionId, PaymentStatus paymentStatus)
//        {
//            try
//            {
//                var session = await _parkingSessionRepository.GetByIdAsync(sessionId);

//                if (session == null)
//                    throw new ArgumentException($"Session with Id {sessionId} not found.");

//                session.PaymentStatus = paymentStatus;
//                await _parkingSessionRepository.UpdateAsync(session);

//                // Send payment confirmation if payment is successful
//                if (paymentStatus == PaymentStatus.Paid)
//                {
//                    _ = Task.Run(async () => {
//                        await SendSessionNotificationAsync(sessionId, NotificationType.Payment);
//                    });
//                }

//                return true;
//            }
//            catch (ArgumentException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error processing payment.", ex);
//            }
//        }

//        // Helper method to send session notifications
//        private async Task SendSessionNotificationAsync(int sessionId, NotificationType notificationType)
//        {
//            try
//            {
//                var session = await _parkingSessionRepository.GetSessionWithDetailsAsync(sessionId);
//                if (session?.User == null) return;

//                var (subject, message) = GenerateEmailContent(session, notificationType);

//                var emailDto = new DTOs.EmailNotification.SendEmailNotificationDto
//                {
//                    UserId = session.UserId,
//                    ParkingSessionId = sessionId,
//                    EmailAddress = session.User.Email,
//                    Subject = subject,
//                    Message = message,
//                    NotificationType = notificationType
//                };

//                await _emailNotificationService.SendNotificationAsync(emailDto);
//            }
//            catch (Exception ex)
//            {
//                // Log error but don't throw to avoid disrupting main operations
//                Console.WriteLine($"Error sending notification: {ex.Message}");
//            }
//        }

//        //private (string subject, string message) GenerateEmailContent(ParkingSession session, NotificationType notificationType)
//        //{
//        //    var userName = $"{session.User?.FirstName} {session.User?.LastName}";
//        //    var slotNumber = session.ParkingSlot?.SlotNumber ?? "Unknown";
//        //    var licensePlate = session.Vehicle?.LicensePlate ?? "Unknown";

//        //    return notificationType switch
//        //    {
//        //        NotificationType.Entry => (
//        //            "Parking Session Started - Smart Parking System",
//        //            $"Dear {userName}, your parking session has started. Vehicle: {licensePlate}, Slot: {slotNumber}, Time: {session.EntryTime:yyyy-MM-dd HH:mm}"
//        //        ),
//        //        NotificationType.Exit => (
//        //            "Parking Session Completed - Smart Parking System",
//        //            $"Dear {userName}, your parking session has ended. Vehicle: {licensePlate}, Slot: {slotNumber}, Duration: {(session.ExitTime - session.EntryTime)?.ToString(@"hh\:mm")} hours, Fee: ${session.ParkingFee:F2}"
//        //        ),
//        //        NotificationType.Payment => (
//        //            "Payment Confirmation - Smart Parking System",
//        //            $"Dear {userName}, payment confirmed for ${session.ParkingFee:F2}. Vehicle: {licensePlate}, Session: {session.Id}"
//        //        ),
//        //        NotificationType.Reservation => ( // 👈 NEW
//        //            "Reservation Confirmed - Smart Parking System",
//        //            $"Dear {userName}, your parking reservation is confirmed.\n\nDetails:\n- Vehicle: {licensePlate}\n- Slot: {slotNumber}\n- Reserved At: {session.ReservedTime:yyyy-MM-dd HH:mm}\n- Planned Entry: {session.EntryTime:yyyy-MM-dd HH:mm}\n- Session ID: {session.Id}\n\nThank you for booking with Smart Parking System!"
//        //        ),
//        //        _ => ("Smart Parking System Notification", $"Dear {userName}, notification about your parking session.")
//        //    };
//        //}

//        private (string subject, string message) GenerateEmailContent(ParkingSession session, NotificationType notificationType)
//        {
//            var userName = $"{session.User?.FirstName} {session.User?.LastName}";
//            var slotNumber = session.ParkingSlot?.SlotNumber ?? "Unknown";
//            var licensePlate = session.Vehicle?.LicensePlate ?? "Unknown";

//            // Convert all UTC times to IST before formatting
//            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
//            var entryTimeIst = TimeZoneInfo.ConvertTimeFromUtc(session.EntryTime, istTimeZone);
//            var reservedTimeIst = session.ReservedTime.HasValue
//                ? TimeZoneInfo.ConvertTimeFromUtc(session.ReservedTime.Value, istTimeZone)
//                : (DateTime?)null;
//            var exitTimeIst = session.ExitTime.HasValue
//                ? TimeZoneInfo.ConvertTimeFromUtc(session.ExitTime.Value, istTimeZone)
//                : (DateTime?)null;

//            return notificationType switch
//            {
//                NotificationType.Reservation => (
//     "Reservation Confirmed - Smart Parking System",
//     $"Dear {userName}, your parking reservation is confirmed.\n\n" +
//     $"Vehicle: {licensePlate}\n" +
//     $"Slot: {slotNumber}\n" +
//     $"Planned Entry (IST): {entryTimeIst:yyyy-MM-dd HH:mm}\n" +
//     (exitTimeIst.HasValue ? $"Planned Exit (IST): {exitTimeIst:yyyy-MM-dd HH:mm}\n" : "") +
//     $"Session ID: {session.Id}\n\n" +
//     "Thank you for booking with Smart Parking System!"
// ),
//                NotificationType.Entry => (
//                    "Parking Session Started - Smart Parking System",
//                    $"Dear {userName}, your parking session has started.\n\n" +
//                    $"Vehicle: {licensePlate}\n" +
//                    $"Slot: {slotNumber}\n" +
//                    $"Entry Time (IST): {entryTimeIst:yyyy-MM-dd HH:mm}"
//                ),
//                NotificationType.Exit => (
//                    "Parking Session Completed - Smart Parking System",
//                    $"Dear {userName}, your parking session has ended.\n\n" +
//                    $"Vehicle: {licensePlate}\n" +
//                    $"Slot: {slotNumber}\n" +
//                    $"Entry Time (IST): {entryTimeIst:yyyy-MM-dd HH:mm}\n" +
//                    $"Exit Time (IST): {exitTimeIst:yyyy-MM-dd HH:mm}\n" +
//                    $"Duration: {(exitTimeIst - entryTimeIst)?.ToString(@"hh\:mm")} hours\n" +
//                    $"Fee: ₹{session.ParkingFee:F2}"
//                ),
//                NotificationType.Payment => (
//                    "Payment Confirmation - Smart Parking System",
//                    $"Dear {userName}, payment confirmed.\n\n" +
//                    $"Vehicle: {licensePlate}\n" +
//                    $"Amount Paid: ₹{session.ParkingFee:F2}\n" +
//                    $"Session ID: {session.Id}"
//                ),
//                _ => ("Smart Parking System Notification",
//                      $"Dear {userName}, notification about your parking session.")
//            };
//        }




//        // Rest of your existing methods remain unchanged...
//        public async Task<IEnumerable<ParkingSessionResponseDto>> GetUnpaidSessionsAsync()
//        {
//            try
//            {
//                var sessions = await _parkingSessionRepository.GetUnpaidSessionsAsync();
//                var sessionDtos = new List<ParkingSessionResponseDto>();

//                foreach (var session in sessions)
//                {
//                    var dto = await MapToParkingSessionResponseDto(session);
//                    sessionDtos.Add(dto);
//                }

//                return sessionDtos;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error retrieving unpaid sessions.", ex);
//            }
//        }

//        public async Task<IEnumerable<ParkingSessionResponseDto>> GetAllActiveSessionsAsync()
//        {
//            try
//            {
//                var sessions = await _parkingSessionRepository.GetActiveSessionsAsync();
//                var sessionDtos = new List<ParkingSessionResponseDto>();

//                foreach (var session in sessions)
//                {
//                    var dto = await MapToParkingSessionResponseDto(session);
//                    sessionDtos.Add(dto);
//                }

//                return sessionDtos;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error retrieving active sessions.", ex);
//            }
//        }

//        public async Task<IEnumerable<ParkingSessionResponseDto>> GetAllReservationsAsync()
//        {
//            try
//            {
//                var sessions = await _parkingSessionRepository.GetReservationsAsync();
//                var sessionDtos = new List<ParkingSessionResponseDto>();

//                foreach (var session in sessions)
//                {
//                    var dto = await MapToParkingSessionResponseDto(session);
//                    sessionDtos.Add(dto);
//                }

//                return sessionDtos;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error retrieving reservations.", ex);
//            }
//        }

//        public async Task<bool> CancelSessionAsync(int sessionId)
//        {
//            try
//            {
//                var session = await _parkingSessionRepository.GetByIdAsync(sessionId);

//                if (session == null)
//                    throw new ArgumentException($"Session with Id {sessionId} not found.");

//                // Free up the slot if it's occupied
//                if (session.Status == SessionStatus.Active || session.Status == SessionStatus.Reserved)
//                {
//                    var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
//                    if (slot != null && slot.IsOccupied)
//                    {
//                        slot.IsOccupied = false;
//                        await _parkingSlotRepository.UpdateAsync(slot);
//                    }
//                }

//                session.Status = SessionStatus.Cancelled;
//                await _parkingSessionRepository.UpdateAsync(session);

//                return true;
//            }
//            catch (ArgumentException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error cancelling session.", ex);
//            }
//        }

//        //public async Task<decimal> CalculateParkingFeeAsync(int sessionId)
//        //{
//        //    try
//        //    {
//        //        var session = await _parkingSessionRepository.GetByIdAsync(sessionId);
//        //        if (session == null)
//        //            throw new ArgumentException($"Session with Id {sessionId} not found.");

//        //        var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId);
//        //        if (vehicle == null)
//        //            return 0;

//        //        var rate = await _parkingRateRepository.GetByVehicleTypeAsync(vehicle.VehicleType);
//        //        if (rate == null)
//        //            return 0;

//        //        var exitTime = session.ExitTime ?? DateTime.UtcNow;
//        //        var duration = exitTime - session.EntryTime;
//        //        var hours = Math.Ceiling(duration.TotalHours);

//        //        return (decimal)hours * rate.HourlyRate;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        throw new Exception("Error calculating parking fee.", ex);
//        //    }
//        //}

//        // Replace the existing CalculateFeeForSession method with this:
//        // Replace the existing CalculateFeeForSession method with this:
//        private async Task<decimal> CalculateParkingFeeAsync(ParkingSession session)
//        {
//            try
//            {
//                VehicleType vehicleType;

//                // Get vehicle type from either Vehicle or Guest
//                if (session.VehicleId.HasValue)
//                {
//                    var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId.Value);
//                    if (vehicle == null) return 0;
//                    vehicleType = vehicle.VehicleType;
//                }
//                else if (session.GuestId.HasValue)
//                {
//                    var guest = await _guestRepository.GetByIdAsync(session.GuestId.Value);
//                    if (guest == null) return 0;
//                    vehicleType = guest.VehicleType;
//                }
//                else
//                {
//                    return 0;
//                }

//                var rate = await _parkingRateRepository.GetByVehicleTypeAsync(vehicleType);
//                if (rate == null) return 0;

//                // Get slot with features for price calculation
//                var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
//                if (slot == null) return 0;

//                // Calculate total price modifier from slot features
//                decimal totalPriceModifier = 1.0m;
//                if (slot.SlotFeatures != null && slot.SlotFeatures.Any())
//                {
//                    foreach (var slotFeature in slot.SlotFeatures)
//                    {
//                        if (slotFeature.Feature != null)
//                        {
//                            totalPriceModifier += slotFeature.Feature.PriceModifier;
//                        }
//                    }
//                }

//                var exitTime = session.ExitTime ?? DateTime.UtcNow;
//                var duration = exitTime - session.EntryTime;
//                var hours = Math.Ceiling(duration.TotalHours);

//                var baseFee = (decimal)hours * rate.HourlyRate;
//                return baseFee * totalPriceModifier;
//            }
//            catch (Exception)
//            {
//                return 0;
//            }
//        }


//        // Dashboard/Reports
//        public async Task<int> GetTotalActiveSlotsAsync()
//        {
//            try
//            {
//                var slots = await _parkingSlotRepository.GetAllAsync();
//                return slots.Count(s => s.IsActive);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error retrieving total active slots.", ex);
//            }
//        }

//        public async Task<int> GetTotalOccupiedSlotsAsync()
//        {
//            try
//            {
//                return await _parkingSessionRepository.GetOccupiedSlotsCountAsync();
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error retrieving total occupied slots.", ex);
//            }
//        }

//        public async Task<decimal> GetTotalRevenueAsync(DateTime fromDate, DateTime toDate)
//        {
//            try
//            {
//                return await _parkingSessionRepository.GetTotalRevenueAsync(fromDate, toDate);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error retrieving total revenue.", ex);
//            }
//        }

//        public async Task<int> GetTotalActiveSessionsCountAsync()
//        {
//            try
//            {
//                return await _parkingSessionRepository.GetActiveSessionsCountAsync();
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error retrieving active sessions count.", ex);
//            }
//        }

//        public async Task<int> GetTotalReservationsCountAsync()
//        {
//            try
//            {
//                var reservations = await _parkingSessionRepository.GetReservationsAsync();
//                return reservations.Count();
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Error retrieving reservations count.", ex);
//            }
//        }

//        // Private mapping methods remain unchanged
//        //private async Task<ParkingSessionResponseDto> MapToParkingSessionResponseDto(ParkingSession session)
//        //{
//        //    var dto = new ParkingSessionResponseDto
//        //    {
//        //        Id = session.Id,
//        //        VehicleId = session.VehicleId,
//        //        VehicleLicensePlate = "",
//        //        SlotId = session.SlotId,
//        //        SlotNumber = "",
//        //        UserId = session.UserId,
//        //        UserName = "",
//        //        EntryTime = session.EntryTime,
//        //        ExitTime = session.ExitTime,
//        //        Status = session.Status,
//        //        ParkingFee = session.ParkingFee,
//        //        PaymentStatus = session.PaymentStatus
//        //    };

//        //    // Get vehicle details
//        //    try
//        //    {
//        //        var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId);
//        //        if (vehicle != null)
//        //        {
//        //            dto.VehicleLicensePlate = vehicle.LicensePlate;
//        //        }
//        //    }
//        //    catch { }

//        //    // Get slot details
//        //    try
//        //    {
//        //        var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
//        //        if (slot != null)
//        //        {
//        //            dto.SlotNumber = slot.SlotNumber;
//        //        }
//        //    }
//        //    catch { }

//        //    // Get user details
//        //    try
//        //    {
//        //        var user = await _userRepository.GetByIdAsync(session.UserId);
//        //        if (user != null)
//        //        {
//        //            dto.UserName = $"{user.FirstName} {user.LastName}";
//        //        }
//        //    }
//        //    catch { }

//        //    return dto;
//        //}
//        // Replace the existing MapToParkingSessionResponseDto method with this:
//        //private async Task<ParkingSessionResponseDto> MapToParkingSessionResponseDto(ParkingSession session)
//        //{
//        //    var dto = new ParkingSessionResponseDto
//        //    {
//        //        Id = session.Id,
//        //        VehicleId = session.VehicleId ?? 0,
//        //        VehicleLicensePlate = "",
//        //        SlotId = session.SlotId,
//        //        SlotNumber = "",
//        //        UserId = session.UserId ?? 0,
//        //        UserName = "",
//        //        EntryTime = session.EntryTime,
//        //        ExitTime = session.ExitTime,
//        //        Status = session.Status,
//        //        ParkingFee = session.ParkingFee,
//        //        PaymentStatus = session.PaymentStatus
//        //    };

//        //    // Handle vehicle details (registered user or guest)
//        //    if (session.VehicleId.HasValue)
//        //    {
//        //        try
//        //        {
//        //            var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId.Value);
//        //            if (vehicle != null)
//        //            {
//        //                dto.VehicleLicensePlate = vehicle.LicensePlate;
//        //            }
//        //        }
//        //        catch { }
//        //    }
//        //    else if (session.GuestId.HasValue)
//        //    {
//        //        try
//        //        {
//        //            var guest = await _guestRepository.GetByIdAsync(session.GuestId.Value);
//        //            if (guest != null)
//        //            {
//        //                dto.VehicleLicensePlate = guest.LicensePlate;
//        //            }
//        //        }
//        //        catch { }
//        //    }

//        //    // Get slot details
//        //    try
//        //    {
//        //        var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
//        //        if (slot != null)
//        //        {
//        //            dto.SlotNumber = slot.SlotNumber;
//        //        }
//        //    }
//        //    catch { }

//        //    // Handle user details (registered user or guest)
//        //    if (session.UserId.HasValue)
//        //    {
//        //        try
//        //        {
//        //            var user = await _userRepository.GetByIdAsync(session.UserId.Value);
//        //            if (user != null)
//        //            {
//        //                dto.UserName = $"{user.FirstName} {user.LastName}";
//        //            }
//        //        }
//        //        catch { }
//        //    }
//        //    else if (session.GuestId.HasValue)
//        //    {
//        //        try
//        //        {
//        //            var guest = await _guestRepository.GetByIdAsync(session.GuestId.Value);
//        //            if (guest != null)
//        //            {
//        //                dto.UserName = $"{guest.FirstName} {guest.LastName} (Guest)";
//        //            }
//        //        }
//        //        catch { }
//        //    }

//        //    return dto;
//        //}
//        private async Task<ParkingSessionResponseDto> MapToParkingSessionResponseDto(ParkingSession session)
//        {
//            var dto = new ParkingSessionResponseDto
//            {
//                Id = session.Id,
//                VehicleId = session.VehicleId ?? 0,
//                VehicleLicensePlate = "",
//                SlotId = session.SlotId,
//                SlotNumber = "",
//                UserId = session.UserId ?? 0,
//                UserName = "",
//                EntryTime = session.EntryTime,
//                ExitTime = session.ExitTime,
//                Status = session.Status,
//                ParkingFee = session.ParkingFee,
//                PaymentStatus = session.PaymentStatus
//            };

//            // Handle vehicle details (registered user or guest)
//            if (session.VehicleId.HasValue)
//            {
//                try
//                {
//                    var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId.Value);
//                    if (vehicle != null)
//                    {
//                        dto.VehicleLicensePlate = vehicle.LicensePlate;
//                    }
//                }
//                catch { }
//            }
//            else if (session.GuestId.HasValue)
//            {
//                try
//                {
//                    var guest = await _guestRepository.GetByIdAsync(session.GuestId.Value);
//                    if (guest != null)
//                    {
//                        dto.VehicleLicensePlate = guest.LicensePlate;
//                    }
//                }
//                catch { }
//            }

//            // Get slot details
//            try
//            {
//                var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
//                if (slot != null)
//                {
//                    dto.SlotNumber = slot.SlotNumber;
//                }
//            }
//            catch { }

//            // Handle user details (registered user or guest)
//            if (session.UserId.HasValue)
//            {
//                try
//                {
//                    var user = await _userRepository.GetByIdAsync(session.UserId.Value);
//                    if (user != null)
//                    {
//                        dto.UserName = $"{user.FirstName} {user.LastName}";
//                    }
//                }
//                catch { }
//            }
//            else if (session.GuestId.HasValue)
//            {
//                try
//                {
//                    var guest = await _guestRepository.GetByIdAsync(session.GuestId.Value);
//                    if (guest != null)
//                    {
//                        dto.UserName = $"{guest.FirstName} {guest.LastName} (Guest)";
//                    }
//                }
//                catch { }
//            }

//            return dto;
//        }


//        private async Task<ReservationDto> MapToReservationDto(ParkingSession session)
//        {
//            // Convert UTC → IST
//            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
//            var entryTimeIst = TimeZoneInfo.ConvertTimeFromUtc(session.EntryTime, istTimeZone);
//            var exitTimeIst = session.ExitTime.HasValue
//                ? TimeZoneInfo.ConvertTimeFromUtc(session.ExitTime.Value, istTimeZone)
//                : (DateTime?)null;

//            var dto = new ReservationDto
//            {
//                Id = session.Id,
//                VehicleLicensePlate = "",
//                SlotNumber = "",
//                PlannedEntryTime = entryTimeIst,   // ✅ Planned entry
//                PlannedExitTime = exitTimeIst,     // ✅ Planned exit
//                Status = session.Status,
//                EstimatedFee = session.ParkingFee
//            };

//            // Get vehicle details
//            try
//            {
//                var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId);
//                if (vehicle != null)
//                {
//                    dto.VehicleLicensePlate = vehicle.LicensePlate;
//                }
//            }
//            catch { }

//            // Get slot details
//            try
//            {
//                var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
//                if (slot != null)
//                {
//                    dto.SlotNumber = slot.SlotNumber;
//                }
//            }
//            catch { }

//            return dto;
//        }

//    }
//}
