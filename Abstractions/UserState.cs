namespace CarInsuranceTgBot.Abstractions;

public enum UserState
{
    None,
    WaitingForPassport,
    WaitingForVehicleDoc,
    WaitingForConfirmation,
    WaitingForPriceAgreement,
    PolicyIssued
}
