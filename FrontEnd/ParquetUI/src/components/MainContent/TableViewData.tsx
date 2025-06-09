import { Table, Button } from "react-bootstrap";
import type { RowData } from "@/types/RowData";
import toLabelFromProperty from "@/utils/toLabelFromProperty";

interface TableViewDataProps {
    data: RowData[];
    alwaysShow: string[];
    onShowDetails: (row: RowData) => void;
}

export default function TableViewData({ data, alwaysShow, onShowDetails }: TableViewDataProps) {
    const allKeys = data.length > 0 ? Object.keys(data[0]) : [];

    return (
        <div className="table-responsive mb-5 mt-5 pt-3">
            <Table striped bordered hover size="sm">
                <thead>
                    <tr>
                        {allKeys.map(key => (
                            <th
                                key={key}
                                className={
                                    alwaysShow.includes(key)
                                        ? ""
                                        : "d-none d-xl-table-cell"
                                }
                            >
                                {toLabelFromProperty(key)}
                            </th>
                        ))}
                        <th className="d-table-cell d-xl-none">Details</th>
                    </tr>
                </thead>
                <tbody>
                    {data.map((row, idx) => (
                        <tr key={idx}>
                            {allKeys.map(key => (
                                <td
                                    key={key}
                                    className={
                                        alwaysShow.includes(key)
                                            ? ""
                                            : "d-none d-xl-table-cell"
                                    }
                                >
                                    {row[key as keyof RowData]}
                                </td>
                            ))}
                            <td className="d-table-cell d-xl-none">
                                <Button
                                    variant="outline-primary"
                                    size="sm"
                                    onClick={() => onShowDetails(row)}
                                >
                                    Details
                                </Button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </div>
    );
}