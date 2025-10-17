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
                
                var session = await _parkingSessionRepository.GetByIdAsync(id);
                if (session == null)
                    throw new ArgumentException($"Parking session with Id {id} not found.");

                
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

        private async Task<bool> HasTimeConflictAsync(int slotId, DateTime startTime, DateTime? endTime, int? excludeSessionId = null)
        {
            try
            {
                
                var existingSessions = await _parkingSessionRepository.GetAllAsync();
                var slotSessions = existingSessions.Where(s =>
                    s.SlotId == slotId &&
                    (s.Status == SessionStatus.Active || s.Status == SessionStatus.Reserved) &&
                    (!excludeSessionId.HasValue || s.Id != excludeSessionId.Value)
                ).ToList();

               
                var effectiveEndTime = endTime ?? startTime.AddHours(24);

                foreach (var session in slotSessions)
                {
                    var sessionStart = session.EntryTime;
                    var sessionEnd = session.ExitTime ?? session.EntryTime.AddHours(24);

                   
                    bool overlaps = startTime < sessionEnd && effectiveEndTime > sessionStart;

                    if (overlaps)
                    {
                        Console.WriteLine($"Time conflict detected: New booking ({startTime} - {effectiveEndTime}) " +
                                        $"conflicts with Session {session.Id} ({sessionStart} - {sessionEnd})");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking time conflicts: {ex.Message}");
                throw;
            }
        }

       
        public async Task<ParkingSessionResponseDto> BookSlotAsync(int userId, BookSlotDto bookSlotDto)
        {
            try
            {
               
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new ArgumentException($"User with Id {userId} not found.");

               
                var vehicle = await _vehicleRepository.GetByIdAsync(bookSlotDto.VehicleId);
                if (vehicle == null)
                    throw new ArgumentException($"Vehicle with Id {bookSlotDto.VehicleId} not found.");

                if (vehicle.OwnerId != userId)
                    throw new UnauthorizedAccessException("Vehicle does not belong to the user.");

               
                var slot = await _parkingSlotRepository.GetByIdAsync(bookSlotDto.SlotId);
                if (slot == null)
                    throw new ArgumentException($"Parking slot with Id {bookSlotDto.SlotId} not found.");

                if (!slot.IsActive)
                    throw new InvalidOperationException("Parking slot is not active.");

               
                if (await HasTimeConflictAsync(bookSlotDto.SlotId, bookSlotDto.PlannedEntryTime, bookSlotDto.PlannedExitTime))
                    throw new InvalidOperationException("This slot is already reserved or occupied for the requested time period.");

              
                if (await _parkingSessionRepository.HasActiveSessionAsync(bookSlotDto.VehicleId))
                    throw new InvalidOperationException("Vehicle already has an active parking session.");

                
                var session = new ParkingSession
                {
                    VehicleId = bookSlotDto.VehicleId,
                    SlotId = bookSlotDto.SlotId,
                    UserId = userId,
                    EntryTime = bookSlotDto.PlannedEntryTime,
                    ExitTime = bookSlotDto.PlannedExitTime,
                    Status = SessionStatus.Reserved,
                    ReservedTime = DateTime.UtcNow,
                    PaymentStatus = PaymentStatus.Pending,
                    ParkingFee = 0
                };

                var createdSession = await _parkingSessionRepository.AddAsync(session);

               
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
        //public async Task<ParkingSessionResponseDto> ActivateReservationAsync(int activatorUserId, ActivateReservationDto activateDto)
        //{
        //    try
        //    {
        //        var session = await _parkingSessionRepository.GetByIdAsync(activateDto.ReservationId);
        //        if (session == null)
        //            throw new ArgumentException($"Reservation with Id {activateDto.ReservationId} not found.");

        //        var activator = await _userRepository.GetByIdAsync(activatorUserId);
        //        if (activator == null || (activator.Role != UserRole.Admin && activator.Role != UserRole.Guard))
        //            throw new UnauthorizedAccessException("Only Admin or Guard can activate reservations.");

        //        if (session.Status != SessionStatus.Reserved)
        //            throw new InvalidOperationException("Session is not in reserved status.");

        //        var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
        //        if (slot != null)
        //        {
        //            slot.IsOccupied = true;
        //            await _parkingSlotRepository.UpdateAsync(slot);
        //        }

        //        session.Status = SessionStatus.Active;
        //        session.EntryTime = activateDto.ActualEntryTime ?? DateTime.UtcNow;

        //        var updatedSession = await _parkingSessionRepository.UpdateAsync(session);

        //        await SendSessionNotificationAsync(updatedSession.Id, NotificationType.Entry);

        //        return await MapToParkingSessionResponseDto(updatedSession);
        //    }
        //    catch (ArgumentException) { throw; }
        //    catch (UnauthorizedAccessException) { throw; }
        //    catch (InvalidOperationException) { throw; }
        //    catch (Exception ex) { throw new Exception("Error activating reservation.", ex); }
        //}

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
                // Convert UTC to IST by adding 5:30 hours
                var utcTime = activateDto.ActualEntryTime ?? DateTime.UtcNow;
                session.EntryTime = utcTime.AddHours(5).AddMinutes(30); // Store IST time

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

                // Check for time conflicts with reservations
                var currentTime = DateTime.UtcNow;
                if (await HasTimeConflictAsync(guestDto.SlotId, currentTime, null))
                    throw new InvalidOperationException("This slot is currently reserved for another session.");

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
                    EntryTime = currentTime,
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

                // Check for time conflicts with reservations
                var currentTime = DateTime.UtcNow;
                if (await HasTimeConflictAsync(startSessionDto.SlotId, currentTime, null))
                    throw new InvalidOperationException("This slot is currently reserved for another session.");

                if (await _parkingSessionRepository.HasActiveSessionAsync(startSessionDto.VehicleId))
                    throw new InvalidOperationException("Vehicle already has an active parking session.");

                slot.IsOccupied = true;
                await _parkingSlotRepository.UpdateAsync(slot);

                var session = new ParkingSession
                {
                    VehicleId = startSessionDto.VehicleId,
                    SlotId = startSessionDto.SlotId,
                    UserId = vehicle.OwnerId,
                    EntryTime = currentTime,
                    Status = SessionStatus.Active,
                    PaymentStatus = PaymentStatus.Pending,
                    ParkingFee = 0
                };

                var createdSession = await _parkingSessionRepository.AddAsync(session);

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

        //public async Task<ParkingSessionResponseDto> EndSessionAsync(EndParkingSessionDto endDto)
        //{
        //    try
        //    {
        //        var session = await _parkingSessionRepository.GetByIdAsync(endDto.SessionId);
        //        if (session == null)
        //            throw new ArgumentException($"Session with Id {endDto.SessionId} not found.");

        //        if (session.Status != SessionStatus.Active)
        //            throw new InvalidOperationException("Session is not active.");

        //        // Update session
        //        session.ExitTime = endDto.ExitTime ?? DateTime.UtcNow;
        //        session.Status = SessionStatus.Completed;
        //        session.ParkingFee = await CalculateFeeForSession(session);

        //        // Free up the slot
        //        var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId);
        //        if (slot != null)
        //        {
        //            slot.IsOccupied = false;
        //            await _parkingSlotRepository.UpdateAsync(slot);
        //        }

        //        var updatedSession = await _parkingSessionRepository.UpdateAsync(session);

        //        // Send exit notification (works for both users and guests)
        //        await SendSessionNotificationAsync(updatedSession.Id, NotificationType.Exit);

        //        return await MapToParkingSessionResponseDto(updatedSession);
        //    }
        //    catch (ArgumentException) { throw; }
        //    catch (InvalidOperationException) { throw; }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error ending session.", ex);
        //    }
        //}


        public async Task<ParkingSessionResponseDto> EndSessionAsync(EndParkingSessionDto endDto)
        {
            try
            {
                var session = await _parkingSessionRepository.GetByIdAsync(endDto.SessionId);
                if (session == null)
                    throw new ArgumentException($"Session with Id {endDto.SessionId} not found.");

                if (session.Status != SessionStatus.Active)
                    throw new InvalidOperationException("Session is not active.");

                // Convert UTC to IST by adding 5:30 hours
                var utcTime = endDto.ExitTime ?? DateTime.UtcNow;
                session.ExitTime = utcTime.AddHours(5).AddMinutes(30); // Store IST time
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

                //var exitTime = session.ExitTime ?? DateTime.UtcNow;
                //var duration = exitTime - session.EntryTime;
                //var hours = Math.Ceiling(duration.TotalHours);

                //var baseFee = (decimal)hours * rate.HourlyRate;
                //return baseFee * totalPriceModifier;

                var exitTime = session.ExitTime ?? DateTime.UtcNow;
                var duration = exitTime - session.EntryTime;
                var hours = duration.TotalHours <= 0 ? 1 : Math.Ceiling(duration.TotalHours);

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
                
            }
        }

        
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


