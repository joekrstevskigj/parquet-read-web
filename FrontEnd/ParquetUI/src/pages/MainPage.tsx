import { Stack } from 'react-bootstrap';
import 'bootstrap/dist/css/bootstrap.min.css'
import ToolbarTop from '@/components/ToolbarSearchFilter/ToolbarTop';
import MainContentContainer from '@/components/MainContent/MainContentContainer';
import useApi from '@/hooks/useApi';
import { useEffect, useState } from 'react';
import type { EmployeesResponse } from '@/types/EmployeesResponse';
import type { EmployeesRequest } from '@/types/EmployeesRequest';

const defaultRequest: EmployeesRequest = {
  Page: 1,
  PageSize: 50,
};

export default function MainPage() {
  const GET_EMPLOYEES = '/Employees/GetEmployeesData';

  const { data: employeesResponse, loading, error, fetchData } = useApi<EmployeesResponse>(GET_EMPLOYEES,);
  const [request, setRequest] = useState<EmployeesRequest>(defaultRequest);

  useEffect(() => {
    fetchData();
  }, []);

  useEffect(() => {
    fetchData('GET', buildQuery(request));
  }, [request]);

  // Build query string from EmployeesRequest
  const buildQuery = (req: EmployeesRequest) => {
    const params = new URLSearchParams();

    if (req.RegistrationDateFrom) params.append("RegistrationDateFrom", req.RegistrationDateFrom.toISOString());

    if (req.RegistrationDateTo) params.append("RegistrationDateTo", req.RegistrationDateTo.toISOString());

    if (req.Gender) params.append("Gender", req.Gender);
    if (req.Country) params.append("Country", req.Country);
    if (req.Salary) params.append("Salary", req.Salary.toString());

    if (req.BirthDateFrom) params.append("BirthDateFrom", req.BirthDateFrom.toISOString());

    if (req.BirthDateTo) params.append("BirthDateTo", req.BirthDateTo.toISOString());
    if (req.SearchTerm) params.append("SearchTerm", req.SearchTerm);

    Object.keys(req).forEach(key => {
      if (
        key.startsWith("search") &&
        req[key as keyof EmployeesRequest] !== undefined &&
        req[key as keyof EmployeesRequest] !== null
      ) {
        params.append(key, String(req[key as keyof EmployeesRequest]));
      }
    });

    params.append("Page", req.Page?.toString() ?? "1");
    params.append("PageSize", req.PageSize?.toString() ?? "50");

    return params.toString();
  };

  return (
    <Stack gap={2}>
      <ToolbarTop
        setRequest={setRequest}
      />

      <MainContentContainer
        loading={loading}
        error={error}
        employeesResponse={employeesResponse}
        setRequest={setRequest}
      />
    </Stack>
  );
}