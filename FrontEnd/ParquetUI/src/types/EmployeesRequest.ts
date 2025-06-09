export interface EmployeesRequest {
    RegistrationDateFrom?: Date | null;
    RegistrationDateTo?: Date | null;
    Gender?: string | null;
    Country?: string | null;
    Salary?: number | null;
    BirthDateFrom?: Date | null;
    BirthDateTo?: Date | null;
    SearchTerm?: string | null;
    Page?: number;
    PageSize?: number;
    searchFirstName?: boolean;
    searchLastName?: boolean;
    searchEmail?: boolean;
    searchComments?: boolean;
    searchTitle?: boolean;
}